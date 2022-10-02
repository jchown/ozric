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
    public class Comms : OzricObject, IDisposable
    {
        public override string Name => "Comms";

        public static readonly Uri INGRESS_API = new("ws://supervisor/core/websocket");
        public static readonly Uri CORE_API = new("ws://homeassistant:8123/api/websocket");

        public CommsStatus Status => new() { messagePump = messagePumpRunning };

        private readonly Uri uri;
        private WatsonWsClient? client;

        private bool messagePumpRunning;
        
        /// <summary>
        /// All incoming messages that need to be processed 
        /// </summary>
        private readonly BufferBlock<string> receivedMessages;
        private readonly BlockingCollection<ServerEvent> pendingEvents;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<ServerResult>> asyncResults;

        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Our authentication token 
        /// </summary>
        private readonly string llat;

        public delegate void MessageHandler(object o);
        public delegate void JsonHandler(string message);

        private event MessageHandler? sentMessageHandler;
        private event JsonHandler? sentJsonHandler;
        private event JsonHandler? receivedJsonHandler;

        public Comms(string llat): this(CORE_API, llat)
        {
        }

        public Comms(Uri uri, string llat)
        {
            this.uri = uri;
            this.llat = llat;

            receivedMessages = new BufferBlock<string>();
            pendingEvents = new BlockingCollection<ServerEvent>(new ConcurrentQueue<ServerEvent>());
            asyncResults = new ConcurrentDictionary<int, TaskCompletionSource<ServerResult>>();
        }

        private void CreateClient()
        {
            client = new WatsonWsClient(uri);
            client.ConfigureOptions(options => options.SetRequestHeader("User-Agent", "OzricEngine/0.8"));
            client.MessageReceived += OnMessageReceived;
        }

        private async Task Connect()
        {
            if (client == null)
                CreateClient();

            await client!.StartWithTimeoutAsync((int)ReceiveTimeout.TotalSeconds);
        }

        private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            receivedMessages.Post(Encoding.UTF8.GetString(args.Data));
        }

        private void Disconnect()
        {
            if (client == null)
                return;
            
            try
            {
                client.MessageReceived -= OnMessageReceived;
                client.Stop();
                client.Dispose();
            }
            catch
            {
                // Ignore errors when disconnecting
            }
            finally
            {
                client = null;
            }
        }

        public async Task<T?> Receive<T>()
        {
            try
            {
                string json = await receivedMessages.ReceiveAsync(ReceiveTimeout);

                Log(LogLevel.Debug, "<< {0}", json);

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

        public async Task Send<T>(T t)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t) + " is null");

            if (client == null)
                throw new IOException("Not connected");

            sentMessageHandler?.Invoke(t);

            if (t is ClientCommand cc && cc.id == 0)
            {
                lock (sendCommandLock)
                {
                    cc.id = nextCommandID++;
                }
            }

            CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(SendTimeout);

            var json = Json.Serialize(t, t.GetType());

            Log(LogLevel.Debug, ">> {0}", json);
            sentJsonHandler?.Invoke(json);

            await client.SendAsync(json, WebSocketMessageType.Text, cancellation.Token);
        }

        public void Dispose()
        {
            if (client != null)
            {
                Disconnect();
            }
        }

        public async Task Authenticate()
        {
            await Connect();

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

        public virtual async Task<ServerResult> SendCommand<T>(T command, int millisecondsTimeout) where T : ClientCommand
        {
            if (!messagePumpRunning)
                throw new Exception("Message pump not running");

            TaskCompletionSource<ServerResult> result = new TaskCompletionSource<ServerResult>();

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

            return await result.Task;
        }

        /// <summary>
        /// Start the message pump which will continuously receive messages from HA and dispatch to thread-safe queues. 
        /// </summary>
        /// <param name="engine"></param>
        public async Task StartMessagePump(Engine engine)
        {
            await Send(new ClientEventSubscribe());

            await Receive<ServerEventSubscribed>();

            messagePumpRunning = true;

            _ = Task.Run(() => MessagePump(engine));
        }

        private async Task MessagePump(Engine engine)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        Log(LogLevel.Trace, "... waiting for messages ...");

                        var message = await Receive<ServerMessage>();

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
                                await Send(new ClientEventSubscribe());
                                await Receive<ServerEventSubscribed>();

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
                messagePumpRunning = false;
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

        private void HandleMessage(ServerMessage message)
        {
            Log(LogLevel.Trace, "... checking {0} ...", message.type);

            try
            {
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

                            _ = Task.Run(() => obj.SetResult(result));
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
            await Send(new ClientPing());
            while (true)
            {
                var response = await Receive<ServerMessage>();
                if (response == null)
                    throw new Exception("No reply from ping");

                if (response is ServerPong)
                    break;

                //  Oops, this wasn't the reply

                HandleMessage(response);
            }
        }

        /// <summary>
        /// Return and remove any events received. Will wait for the timeout period if none are already queued.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public List<ServerEvent> TakePendingEvents(int millisecondsTimeout)
        {
            if (!messagePumpRunning)
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

        public void OnSentMessage(MessageHandler action)
        {
            sentMessageHandler += action;
        }

        public void OnSentJson(JsonHandler action)
        {
            sentJsonHandler += action;
        }

        public void OnReceivedJson(JsonHandler action)
        {
            receivedJsonHandler += action;
        }
    }
}