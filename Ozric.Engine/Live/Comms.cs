using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Ozric.Engine.Extensions;
using Ozric.Engine.Graph;
using Ozric.Engine.Messages;
using Ozric.Engine.Utils;
using WatsonWebsocket;

namespace Ozric.Engine.Live;

public class Comms : OzricObject, IComms, IDisposable
{
    public override string Name => "Comms";

    public static readonly Uri IngressApi = new("ws://supervisor/core/websocket");
    public static readonly Uri CoreApi = new("ws://192.168.2.48:8123/api/websocket");

    private const int PingTimeoutMilliseconds = 3000;

    public CommsStatus Status => new() { messagePump = _messagePumpRunning };

    private readonly Uri _uri;
    private WatsonWsClient? _client;

    private bool _messagePumpRunning;
        
    /// <summary>
    /// All incoming messages that need to be processed 
    /// </summary>
    private readonly BufferBlock<string> receivedMessages;
    private readonly BlockingCollection<ServerEvent> pendingEvents;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> asyncResults;

    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Our authentication token 
    /// </summary>
    private readonly string llat;

    private event IComms.MessageHandler? SentMessageHandler;
    private event IComms.JsonHandler? SentJsonHandler;
    private event IComms.JsonHandler? ReceivedJsonHandler;

    private int _nextCommandId = 1;
    private readonly Lock _sendLock = new();
    private readonly Lock _sendCommandLock = new();


    public Comms(string llat): this(CoreApi, llat)
    {
    }

    public Comms(Uri uri, string llat)
    {
        this._uri = uri;
        this.llat = llat;

        receivedMessages = new BufferBlock<string>();
        pendingEvents = new BlockingCollection<ServerEvent>(new ConcurrentQueue<ServerEvent>());
        asyncResults = new ConcurrentDictionary<int, TaskCompletionSource<string>>();
    }

    private void CreateClient()
    {
        _client = new WatsonWsClient(_uri);
        _client.ConfigureOptions(options =>options.SetRequestHeader("User-Agent", "Ozric.Engine/0.8"));
        _client.MessageReceived += OnMessageReceived;
    }

    public async Task Connect()
    {
        if (_client == null)
            CreateClient();

        await _client!.StartWithTimeoutAsync((int)ReceiveTimeout.TotalSeconds);
        await Authenticate();
        await Subscribe();
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
    {
        var message = Encoding.UTF8.GetString(args.Data);
        receivedMessages.Post(message);
    }

    private void Disconnect()
    {
        if (_client == null)
            return;
            
        try
        {
            _client.MessageReceived -= OnMessageReceived;
            _client.Stop();
            _client.Dispose();
        }
        catch
        {
            // Ignore errors when disconnecting
        }
        finally
        {
            _client = null;
        }
    }

    private async Task<T?> Receive<T>()
    {
        try
        {
            var json = await ReceiveJsonAsync();

            try
            {
                var t = Json.Deserialize<T>(json);
                ReceivedJsonHandler?.Invoke(json);
                return t;
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing {json}");
            }
        }
        catch (TimeoutException)
        {
            return default;
        }
    }

    private async Task<string> ReceiveJsonAsync()
    {
        var json = await receivedMessages.ReceiveAsync(ReceiveTimeout);

        Log(LogLevel.Debug, "<< {0}", json);
        return json;
    }
    
    private async Task SendAsync<T>(T t)
    {
        if (t == null)
            throw new ArgumentNullException(nameof(t) + " is null");

        if (_client == null)
            throw new IOException("Not connected");

        SentMessageHandler?.Invoke(t);

        if (t is ClientCommand cc && cc.id == 0)
        {
            lock (_sendCommandLock)
            {
                cc.id = _nextCommandId++;
            }
        }

        var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(SendTimeout);

        var json = Json.Serialize(t, t.GetType());

        Log(LogLevel.Debug, ">> {0}", json);
        SentJsonHandler?.Invoke(json);

        Task sendTask;
            
        lock (_sendLock)
        {
            sendTask = _client.SendAsync(json, WebSocketMessageType.Text, cancellation.Token);
        }

        await sendTask;
    }

    private void Send<TMessage>(TMessage t)
    {
        if (t == null)
            throw new ArgumentNullException(nameof(t));

        if (_client == null)
            throw new IOException("Not connected");

        SentMessageHandler?.Invoke(t);

        if (t is ClientCommand cc && cc.id == 0)
        {
            lock (_sendCommandLock)
            {
                cc.id = _nextCommandId++;
            }
        }

        var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(SendTimeout);

        var json = Json.Serialize(t, t.GetType());

        Log(LogLevel.Debug, ">> {0}", json);
        SentJsonHandler?.Invoke(json);

        lock (_sendLock)
        {
            var token = cancellation.Token;
            _ = Task.Run(() => _client.SendAsync(json, WebSocketMessageType.Text, token), token);
        }
    }

    public void Dispose()
    {
        if (_client != null)
        {
            Disconnect();
        }
    }

    /// <summary>
    /// Initial handshake with the Home Assistant server.
    /// </summary>
        
    public async Task Authenticate()
    {
        var authReq = await Receive<ServerAuthRequired>() ?? throw new Exception("No authentication challenge");
        Log(LogLevel.Info, "Auth requested by HA {0}", authReq.ha_version);

        var auth = new ClientAuth(accessToken: llat);
        await SendAsync(auth);

        var authResult = await Receive<ServerMessage>();
        switch (authResult)
        {
            case ServerAuthOK:
                Log(LogLevel.Info, "Auth OK");
                break;

            case ServerAuthInvalid invalid:
                throw new Exception($"Auth failed: {invalid.message}");

            default:
                throw new Exception($"Auth failed: Unexpected result: {authResult}");
        }
    }

    public virtual async Task<TResponse> SendCommand<TResponse>(ClientCommand command, int millisecondsTimeout = IComms.DefaultCommandTimeoutMilliseconds, bool verbose = false) where TResponse: ServerResponse, new()
    {
        if (!_messagePumpRunning)
            throw new Exception("Message pump not running");

        var completionSource = new TaskCompletionSource<string>();
        Task send;

        lock (_sendCommandLock)
        {
            command.id = _nextCommandId++;

            if (!asyncResults.TryAdd(command.id, completionSource))
                Log(LogLevel.Error, "Failed to register result handler for command {0}", command.id);

            Log(LogLevel.Info, "Sending command: ID {0}, Type \"{1}\"", command.id, command.type);
            send = SendAsync(command);
        }

        await send;

        using var timeout = new CancellationTokenSource(millisecondsTimeout);
        try
        {
            var response = await completionSource.Task.WaitAsync(timeout.Token);

            if (verbose)
            {
                Log(LogLevel.Info, "Received {0}", Json.Prettify(response));
            }

            var result = Json.Deserialize<TResponse>(response);

            if (command.id != result.id)
            {
                throw new Exception($"Command ID mismatch: sent {command.id}, received {result.id}");
            }

            Log(LogLevel.Debug, "Received response: ID {0}, {1}", result.id, result.Describe());
            return result;
        }
        catch (OperationCanceledException)
        {
            throw new Exception($"Timeout waiting for response to {command}");
        }
    }

    /// <summary>
    /// Start the message pump which will continuously receive messages from HA and dispatch to thread-safe queues. 
    /// </summary>
    private async Task Subscribe()
    {
        await SendSubscribe();

        _messagePumpRunning = true;

        Tasks.Run(MessagePump);
    }

    private async Task SendSubscribe()
    {
        await SendAsync(new ClientEventSubscribe());
        var result = await Receive<ServerResult>() ?? throw new Exception("No result for subscribe command");
        if (!result.success)
            throw new Exception($"Failed to subscribe to events: {result.Describe()}");
    }

    private async Task MessagePump()
    {
        try
        {
            while (true)
            {
                try
                {
                    Log(LogLevel.Trace, "... waiting for messages ...");

                    try
                    {
                        var message = await ReceiveJsonAsync();
                        HandleMessage(message);
                    }
                    catch (TimeoutException)
                    {
                        lock (_sendLock)
                        {
                            Send(new ClientPing());
                            var receive = Receive<ServerPong>();
                            if (receive == null)
                            {
                                throw new TimeoutException("No pong received from server");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(LogLevel.Error, "Error receiving message: {0}", e);

                    while (true)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                            
                        try
                        {
                            DiscardPendingMessages();

                            Log(LogLevel.Info, "Reconnecting...");

                            Disconnect();

                            await Authenticate();
                            await SendSubscribe();
                                
                            Log(LogLevel.Info, "Reconnected");
                            break;
                        }
                        catch (Exception re)
                        {
                            Log(LogLevel.Info, "Reconnect failed: {0}", re);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log(LogLevel.Error, "Exception thrown in message pump: {0}", e);
        }
        finally
        {
            _messagePumpRunning = false;
        }
    }
        
    /// <summary>
    /// Throw away all buffered messages
    /// </summary>

    private void DiscardPendingMessages()
    {
        while (receivedMessages.TryReceive(out _))
        {
        }
    }

    private void HandleMessage(string json)
    {
        try
        {
            var message = Json.Deserialize<ServerMessage>(json);
            switch (message)
            {
                case ServerEvent ev:
                {
                    pendingEvents.Add(ev);
                    break;
                }

                case ServerResult result:
                {
                    if (asyncResults.TryGetValue(result.id ?? -1, out var obj))
                    {
                        Log(LogLevel.Trace, "Got result for command {0}", result.id);

                        _ = Task.Run(() => obj.SetResult(json));
                    }
                    else
                    {
                        Log(LogLevel.Warning, "No task waiting for result of client message ID {0}, ignored", result.id);
                    }
                    break;
                }

                case ServerAuthRequired:
                case ServerAuthOK:
                {
                    //  These messages should be handled by Authenticate(). Server forgot about us?

                    throw new Exception("Authentication required");
                }

                default:
                {
                    Log(LogLevel.Warning, "Unknown message type ({0}), ignored", message.type);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Log(LogLevel.Error, "Error handling message: {0}", e);
        }
    }

    /// <summary>
    /// Return and remove any events received. Will wait for the timeout period if none are already queued.
    /// </summary>
    /// <param name="millisecondsTimeout"></param>
    /// <returns></returns>
    public List<ServerEvent> TakePendingEvents(int millisecondsTimeout)
    {
        if (!_messagePumpRunning)
            throw new Exception("Message pump not running");

        var taken = new List<ServerEvent>();

        if (pendingEvents.TryTake(out var ev1, millisecondsTimeout))
        {
            taken.Add(ev1);

            while (pendingEvents.TryTake(out var ev2))
                taken.Add(ev2);
        }

        return taken;
    }

    public void OnSentMessage(IComms.MessageHandler action)
    {
        SentMessageHandler += action;
    }

    public void OnSentJson(IComms.JsonHandler action)
    {
        SentJsonHandler += action;
    }

    public void OnReceivedJson(IComms.JsonHandler action)
    {
        ReceivedJsonHandler += action;
    }
}