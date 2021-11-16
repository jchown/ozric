using System;
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
                
                await engine.Send(new ClientEventSubscribe());
                
                await engine.Receive<ServerEventSubscribed>();

                while (true)
                {
                    var ev = await engine.Receive<ServerMessageEvent>();

                    if (ev.payload is EventStateChanged stateChanged)
                    {
                        Console.WriteLine($"{stateChanged.data.new_state.entity_id} = {stateChanged.data.old_state.state} -> {stateChanged.data.new_state.state}");
                    }
                    
                    if (ev.payload is EventCallService callService)
                    {
                        Console.WriteLine($"{callService.data.domain}: {callService.data.service_data.entity_id}: {callService.data.service}");
                    }
                }
            }
        }
    }
}