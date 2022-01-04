using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OzricEngine
{
    public class Comms : IDisposable
    {
        private Uri uri = new Uri("ws://homeassistant:8123/api/websocket");

        private ClientWebSocket client = new ClientWebSocket();
//              client("User-Agent", "Ozric/0.1");

        private byte[] buffer = new byte[65536];
        private bool messagePumpRunning;
        private readonly BlockingCollection<ServerEvent> pendingEvents;
        private readonly ConcurrentDictionary<int, AsyncObject<ServerResult>> asyncResults;

        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonConverterServerMessage(), new JsonConverterEvent() }
        };

        public Comms()
        {
            pendingEvents = new BlockingCollection<ServerEvent>(new ConcurrentQueue<ServerEvent>());
            asyncResults = new ConcurrentDictionary<int, AsyncObject<ServerResult>>();
        }

        private async Task Connect()
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(ReceiveTimeout);

            await client.ConnectAsync(uri, cancellation.Token);
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

            WriteLine($"<< {json}");
            
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        
        public async Task Send<T>(T t)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(SendTimeout);

            var json = JsonSerializer.Serialize(t, JsonOptions);
            WriteLine($">> {json}");

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

        public async Task Authenticate(string llat)
        {
            await Connect();
            
            var authReq = await Receive<ServerAuthRequired>();
            Console.WriteLine($"Auth requested by HA {authReq.ha_version}");

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
        
        public async Task<ServerResult> SendCommand<T>(T command, int millisecondsTimeout) where T : ClientCommand
        {
            if (!messagePumpRunning)
                throw new Exception("Message pump not running");

            var receiver = WaitForResult(command.id, millisecondsTimeout);

            await Send(command);

            return await receiver;
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
                                obj.Set(result);
                            }
                            else
                            {
                                engine.Log($"No task waiting for result of client message {result.id}, ignored");
                            }

                            break;
                        }

                        default:
                        {
                            engine.Log($"Unknown message type ({message.type}), ignored");
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                engine.Log($"Exception thrown in message pump: {e}");
            }
            finally
            {
                messagePumpRunning = false;
            }
        }

        /// <summary>
        /// Start waiting for a ServerResult with the id given. The message pump must have been started.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>

        private async Task<ServerResult> WaitForResult(int id, int millisecondsTimeout)
        {
            if (!messagePumpRunning)
                throw new Exception("Message pump not running");

            var obj = new AsyncObject<ServerResult>();
            
            if (!asyncResults.TryAdd(id, obj))
                return null;
            
            return await obj.Get(millisecondsTimeout);
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