using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OzricEngine
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using (var engine = new Engine())
            {
                await engine.Authenticate();
            }
        }
    }
}