using System;
using System.IO;
using System.Threading.Tasks;
using OzricEngine;
using OzricEngine.logic;

namespace OzricAddon
{
    class Program
    {
        public class Options
        {
            public string token { get; set; }
            public string graph { get; set; }
        }
            
        public static async Task Main(string[] args)
        {
            try
            {
                var optionsJson = File.ReadAllText("/data/options.json");

                Console.WriteLine(Json.Prettify(optionsJson));

                var options = Json.Deserialize<Options>(optionsJson);
                
                var graph = Json.Deserialize<Graph>(options.graph);

                using (var connection = new Comms(options.token))
                {
                    await connection.Authenticate();
                
                    await connection.Send(new ClientGetStates());

                    var states = await connection.Receive<ServerGetStates>();

                    var home = new Home(states.result);

                    var engine = new Engine(home, graph, connection);

                    await engine.MainLoop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}