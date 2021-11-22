using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using (var connection = new Connection())
            {
                await connection.Authenticate();
                
                await connection.Send(new ClientGetStates());
                
                var states = await connection.Receive<ServerGetStates>();

                var home = new Home(states.result);
                
                var engine = new Engine(home, connection);

                await engine.ProcessEvents();

                await TogglePanasonic(connection);

                //await ProcessEvents(engine);
            }
        }

        private static async Task TogglePanasonic(Connection connection)
        {
            var callServices = new ClientCallService
            {
                domain = "light",
                service = "turn_off",
                target = new Dictionary<string, string>()
                {
                    { "entity_id", "light.panasonic_strip" }
                }
            };
            await connection.Send(callServices);

            await connection.Receive<ServerMessage>();
        }
    }
}