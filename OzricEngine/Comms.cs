using System;
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

        private string llat =
            "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiI5NjIyODE0NmFmMmQ0YTVmOWZiZmNiNDRmNTY0ZGQ4NSIsImlhdCI6MTYzNzAwMzc4OCwiZXhwIjoxOTUyMzYzNzg4fQ.YeZGrm3Shnx5Zu8MedejVB61t2GWWr4gU0MqIqb0cXY";

        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonConverterServerMessage(), new JsonConverterEvent() }
        };

        public async Task Connect()
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

        public async Task Authenticate()
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
    }
}