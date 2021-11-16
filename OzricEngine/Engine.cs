using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OzricEngine
{
    public class Engine : IDisposable
    {
        private Uri uri = new Uri("ws://homeassistant:8123/api/websocket");

        private ClientWebSocket client = new ClientWebSocket();
//              client("User-Agent", "Ozric/0.1");

        private byte[] buffer = new byte[65536];

        private string llat =
            "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiI5NjIyODE0NmFmMmQ0YTVmOWZiZmNiNDRmNTY0ZGQ4NSIsImlhdCI6MTYzNzAwMzc4OCwiZXhwIjoxOTUyMzYzNzg4fQ.YeZGrm3Shnx5Zu8MedejVB61t2GWWr4gU0MqIqb0cXY";

        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
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

            var received = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation.Token);
            var json = Encoding.UTF8.GetString(buffer, 0, received.Count);

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
            
            var authReq = await Receive<ServerMessageAuthRequired>();
            Console.WriteLine($"Auth requested by HA {authReq.ha_version}");

            var auth = new ClientAuth
            {
                access_token = llat
            };
            await Send(auth);
            
            var authResult = await Receive<ServerMessage>();
            if (authResult is ServerAuthInvalid invalid)
            {
                throw new Exception($"Auth failed: {invalid.message}");
            }
            if (!(authResult is ServerMessageAuthOK))
            {
                throw new Exception($"Auth failed: Unexpected result: {authResult}");
            }
            Console.WriteLine("Auth OK");
        }
    }
}