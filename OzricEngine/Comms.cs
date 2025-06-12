using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using OzricEngine.ext;
using OzricEngine.Nodes;
using WatsonWebsocket;

namespace OzricEngine
{
    public class Comms : OzricObject, IComms, IDisposable
    {
        public override string Name => "Comms";

        public static readonly Uri INGRESS_API = new("ws://supervisor/core/websocket");
        //public static readonly Uri CORE_API = new("ws://homeassistant.local:8123/api/websocket");
        public static readonly Uri CORE_API = new("ws://192.168.2.48:8123/api/websocket");

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

        private event IComms.MessageHandler? sentMessageHandler;
        private event IComms.JsonHandler? sentJsonHandler;
        private event IComms.JsonHandler? receivedJsonHandler;

        public Comms(string llat): this(CORE_API, llat)
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
            _client.ConfigureOptions(options =>options.SetRequestHeader("User-Agent", "OzricEngine/0.8"));
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
            receivedMessages.Post(Encoding.UTF8.GetString(args.Data));
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
                var json = await ReceiveJson();

                try
                {
                    var t = Json.Deserialize<T>(json);
                    receivedJsonHandler?.Invoke(json);
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

        private async Task<string> ReceiveJson()
        {
            var json = await receivedMessages.ReceiveAsync(ReceiveTimeout);

            Log(LogLevel.Debug, "<< {0}", json);
            return json;
        }

        public async Task Send<T>(T t)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t) + " is null");

            if (_client == null)
                throw new IOException("Not connected");

            sentMessageHandler?.Invoke(t);

            if (t is ClientCommand cc && cc.id == 0)
            {
                lock (sendCommandLock)
                {
                    cc.id = nextCommandID++;
                }
            }

            var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(SendTimeout);

            var json = Json.Serialize(t, t.GetType());

            Log(LogLevel.Debug, ">> {0}", json);
            sentJsonHandler?.Invoke(json);

            await _client.SendAsync(json, WebSocketMessageType.Text, cancellation.Token);
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
            await Send(auth);

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

        private static int nextCommandID = 1;
        private static readonly object sendCommandLock = new();

        public virtual async Task<TResponse> SendCommand<TResponse>(ClientCommand command, int millisecondsTimeout = IComms.DefaultCommandTimeoutMilliseconds) where TResponse: ServerResponse, new()
        {
            if (!_messagePumpRunning)
                throw new Exception("Message pump not running");

            var result = new TaskCompletionSource<string>();
            Task send;

            lock (sendCommandLock)
            {
                command.id = nextCommandID++;

                if (!asyncResults.TryAdd(command.id, result))
                    Log(LogLevel.Error, "Failed to register result handler for command {0}", command.id);

                Log(LogLevel.Info, "Sending command: {0}", command);
                send = Send(command);
            }

            await send;

            var response = await result.Task;
            return Json.Deserialize<TResponse>(response);
        }

        /// <summary>
        /// Start the message pump which will continuously receive messages from HA and dispatch to thread-safe queues. 
        /// </summary>
        private async Task Subscribe()
        {
            await SendSubscribe();

            _messagePumpRunning = true;

            _ = Task.Run(MessagePump);
        }

        private async Task SendSubscribe()
        {
            await Send(new ClientEventSubscribe());
            var result = await Receive<ServerResult>() ?? throw new Exception("No result for subscribe command");
            if (!result.success)
                throw new Exception($"Failed to subscribe to events: {result.DescribeError()}");
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

                        var message = await ReceiveJson();

                        if (message == null)
                        {
                            //  No messages? Check server is still reachable

                            await CheckConnected();
                        }
                        else
                        {
                            HandleMessage(message);
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
                                FlushPendingMessages();

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

        private void FlushPendingMessages()
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

        protected virtual async Task CheckConnected()
        {
            await SendCommand<ServerPong>(new ClientPing(), PingTimeoutMilliseconds);
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
            sentMessageHandler += action;
        }

        public void OnSentJson(IComms.JsonHandler action)
        {
            sentJsonHandler += action;
        }

        public void OnReceivedJson(IComms.JsonHandler action)
        {
            receivedJsonHandler += action;
        }
    }
}