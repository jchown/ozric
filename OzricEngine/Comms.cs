using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using OzricEngine.ext;
using OzricEngine.logic;
using WatsonWebsocket;

namespace OzricEngine
{
    public class Comms : OzricObject, IDisposable
    {
        public override string Name => "Comms";

        public CommsStatus Status => new() { messagePump = messagePumpRunning };

        private Uri uri = new("ws://homeassistant:8123/api/websocket");

        private WatsonWsClient client;

        private bool messagePumpRunning;
        private readonly BufferBlock<string> receivedMessages;
        private readonly BlockingCollection<ServerEvent> pendingEvents;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<ServerResult>> asyncResults;

        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

        private readonly string llat;

        public Comms(string llat)
        {
            this.llat = llat;

            receivedMessages = new BufferBlock<string>();
            pendingEvents = new BlockingCollection<ServerEvent>(new ConcurrentQueue<ServerEvent>());
            asyncResults = new ConcurrentDictionary<int, TaskCompletionSource<ServerResult>>();
        }

        private async Task Connect()
        {
            client = new WatsonWsClient(uri);
            client.ConfigureOptions(options => options.SetRequestHeader("User-Agent", "OzricEngine/0.7"));
            client.MessageReceived += OnMessageReceived;

            await client.StartWithTimeoutAsync((int)ReceiveTimeout.TotalSeconds);
        }

        private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            receivedMessages.Post(Encoding.UTF8.GetString(args.Data));
        }

        private void Disconnect()
        {
            try
            {
                client.Stop();
                client.Dispose();
            }
            catch (Exception)
            {
            }
            finally
            {
                client = null;
            }
        }

        public async Task<T> Receive<T>()
        {
            try
            {
                string json = await receivedMessages.ReceiveAsync(ReceiveTimeout);

                Log(LogLevel.Debug, "<< {0}", json);

                try
                {
                    var t = Json.Deserialize<T>(json);
                    receiveHandler?.Invoke(json);
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
            sendHandler?.Invoke(json);

            await client.SendAsync(json, WebSocketMessageType.Text, cancellation.Token);
        }

        private void WriteLine(string s)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(s);
            Console.ForegroundColor = before;
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        public async Task Authenticate()
        {
            await Connect();

            var authReq = await Receive<ServerAuthRequired>();
            Log(LogLevel.Info, "Auth requested by HA {0}", authReq.ha_version);

            var auth = new ClientAuth(accessToken: llat);
            await Send(auth);

            var authResult = await Receive<ServerMessage>();
            switch (authResult)
            {
                case ServerAuthOK _:
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

                            await PingPong();
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
                                Log(LogLevel.Info, "Reconnecting...");

                                Disconnect();

                                await Authenticate();
                                await Send(new ClientEventSubscribe());
                                await Receive<ServerEventSubscribed>();

                                Log(LogLevel.Info, "Reconnected");
                                break;
                            }
                            catch (Exception)
                            {
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

        private async Task PingPong()
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

        public delegate void MessageHandler(string message);

        private MessageHandler sendHandler, receiveHandler;

        public void OnSend(MessageHandler action)
        {
            sendHandler += action;
        }

        public void OnReceive(MessageHandler action)
        {
            receiveHandler += action;
        }
    }

    public class CommsStatus
    {
        public bool messagePump { get; set; }
    }
}