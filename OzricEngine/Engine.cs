using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.logic;

namespace OzricEngine
{
    public class Engine
    {
        private readonly Home home;
        private readonly Connection connection;
        private readonly Dictionary<string, Node> nodes;

        public Engine(Home home, Connection connection)
        {
            this.home = home;
            this.connection = connection;
            
            nodes = new Dictionary<string, Node>();
            Add(new SkyBrightness());

            foreach (var node in nodes.Values)
            {
                node.OnInit(home);
            }
        }

        private void Add(Node node)
        {
            nodes[node.id] = node;
        }

        public async Task ProcessEvents()
        {
            await connection.Send(new ClientEventSubscribe());

            await connection.Receive<ServerEventSubscribed>();

            while (true)
            {
                var ev = await connection.Receive<ServerEvent>();

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