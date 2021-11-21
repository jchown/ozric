using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using (var engine = new Engine())
            {
                await engine.Authenticate();
                
                await engine.Send(new ClientGetStates());
                
                var states = await engine.Receive<ServerGetStates>();
                
                Console.WriteLine(states.result.Select(entity => $"{entity.Name} {entity.state}").Join("\n"));

                var callServices = new ClientCallService
                {
                    domain = "light",
                    service = "turn_off",
                    target = new Dictionary<string, string>()
                    {
                        { "entity_id", "light.panasonic_strip" }
                    }
                };
                await engine.Send(callServices);

                await engine.Receive<ServerMessage>();

                //await ProcessEvents(engine);
            }
        }

        private static async Task ProcessEvents(Engine engine)
        {
            await engine.Send(new ClientEventSubscribe());

            await engine.Receive<ServerEventSubscribed>();

            while (true)
            {
                var ev = await engine.Receive<ServerEvent>();

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