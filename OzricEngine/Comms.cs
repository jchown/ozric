using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    public class Comms: OzricObject, IDisposable
    {
        public override string Name => "Comms";

        private Uri uri = new Uri("ws://homeassistant:8123/api/websocket");

        private ClientWebSocket client;

        private byte[] buffer = new byte[65536];
        private bool messagePumpRunning;
        private readonly BlockingCollection<ServerEvent> pendingEvents;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<ServerResult>> asyncResults;

        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            Converters = { new JsonConverterServerMessage(), new JsonConverterEvent() }
        };

        private readonly string llat;

        public Comms(string llat)
        {
            this.llat = llat;
            
            pendingEvents = new BlockingCollection<ServerEvent>(new ConcurrentQueue<ServerEvent>());
            asyncResults = new ConcurrentDictionary<int, TaskCompletionSource<ServerResult>>();
            minLogLevel = LogLevel.Debug;
        }

        private async Task Connect()
        {
            client = new ClientWebSocket();
//              client("User-Agent", "Ozric/0.1");

            CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(ReceiveTimeout);

            await client.ConnectAsync(uri, cancellation.Token);
        }

        private void Disconnect()
        {
            try
            {
                client.Abort();
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
            CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(ReceiveTimeout);

            string json;
            using (var ms = new MemoryStream())
            {
                var bytes = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult received;
                do
                {
                    received = await client.ReceiveAsync(bytes, cancellation.Token);
                    ms.Write(bytes.Array, bytes.Offset, received.Count);
                }
                while (!received.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }
            }

            Log(LogLevel.Debug,  "<< {0}", json);

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing {json}");
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

            var json = JsonSerializer.Serialize(t, JsonOptions);

            Log(LogLevel.Debug,  ">> {0}", json);

            int length = Encoding.UTF8.GetBytes(json, 0, json.Length, buffer, 0);
            await client.SendAsync(new ArraySegment<byte>(buffer, 0, length), WebSocketMessageType.Text, true, cancellation.Token);
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

            var auth = new ClientAuth
            {
                access_token = llat
            };
            await Send(auth);
            
            var authResult = await Receive<ServerMessage>();
            switch (authResult)
            {
                case ServerAuthOK _:
                    Console.WriteLine("Auth OK");
                    break;
                
                case ServerAuthInvalid invalid:
                    throw new Exception($"Auth failed: {invalid.message}");
                
                default:
                    throw new Exception($"Auth failed: Unexpected result: {authResult}");
            }
        }

        private static int nextCommandID = 1;
        private static object sendCommandLock = new object();

        public async Task<ServerResult> SendCommand<T>(T command, int millisecondsTimeout) where T : ClientCommand
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

                Log(LogLevel.Trace, "Sending command {0}", command.id);
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
                        var message = await Receive<ServerMessage>();

                        switch (message)
                        {
                            case ServerEvent ev:
                            {
                                pendingEvents.Add(ev);
                                break;
                            }

                            case ServerResult result:
                            {
                                if (asyncResults.TryGetValue(result.id, out var obj))
                                {
                                    Log(LogLevel.Trace, "Got result for command {0}", result.id);
                                    
                                    obj.SetResult(result);
                                }
                                else
                                {
                                    Log(LogLevel.Warning, "No task waiting for result of client message {0}, ignored", result.id);
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
                    catch (WebSocketException e)
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
                            catch (WebSocketException)
                            {
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log(LogLevel.Error, "Error handling message: {0}", e);
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
        /// Return and remove any events received. Will wait for the timeout period if none are already queued.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>

        public List<ServerEvent> TakePendingEvents(int millisecondsTimeout)
        {
            if (!messagePumpRunning)
                throw new Exception("Message pump not running");

            var taken = new List<ServerEvent>();

            if (pendingEvents.Count == 0)
            {
                if (pendingEvents.TryTake(out var ev, millisecondsTimeout))
                    taken.Add(ev);
            }

            while (pendingEvents.TryTake(out var ev))
                taken.Add(ev);

            return taken;
        }
    }
}