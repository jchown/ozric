using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    public class Engine
    {
        private readonly Home home;
        private readonly Comms comms;
        private readonly Dictionary<string, Node> nodes;
        private readonly Dictionary<OutputSelector, List<InputSelector>> edges;

        public Engine(Home home, Comms comms)
        {
            this.home = home;
            this.comms = comms;
            
            nodes = new Dictionary<string, Node>();
            edges = new Dictionary<OutputSelector, List<InputSelector>>();
        }

        public void Add(Node node)
        {
            nodes[node.id] = node;
        }

        public void Connect(OutputSelector output, InputSelector input)
        {
            var inputs = edges.GetOrSet(output, () => new List<InputSelector>());
            inputs.Add(input);
            
            Console.WriteLine($"{output.nodeID}.{output.outputName} -> {input.nodeID}.{input.inputName}");
        }

        public void Disconnect(OutputSelector output, InputSelector input)
        {
            edges.Get(output).Remove(input);
        }

        public async Task ProcessEvents()
        {
            foreach (var node in nodes.Values)
            {
                node.OnInit(home);
            }

            await comms.Send(new ClientEventSubscribe());

            await comms.Receive<ServerEventSubscribed>();

            while (true)
            {
                var ev = await comms.Receive<ServerEvent>();

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

        public void Update()
        {
            foreach (var nodeID in GetNodesInOrder())
            {
                var node = nodes[nodeID];
                
                node.OnUpdate(home);

                foreach (var edge in edges)
                {
                    if (edge.Key.nodeID != nodeID)
                        continue;

                    var value = node.GetOutputValue(edge.Key.outputName);

                    foreach (var input in edge.Value)
                    {
                        home.Log($"{input.nodeID}.{input.inputName} = {value}");
                        nodes[input.nodeID].SetInputValue(input.inputName, value);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the nodes in update order, such that all inputs
        /// can be read after being written by their upstream outputs.
        /// </summary>
        /// <returns></returns>

        public List<string> GetNodesInOrder()
        {
            //  Work out the dependencies for each node

            var dependencies = new Dictionary<string, List<string>>();
            foreach (var edge in edges)
            {
                var output = edge.Key.nodeID;
                
                foreach (var input in edge.Value)
                    dependencies.GetOrSet(input.nodeID, () => new List<string>()).Add(output);
            }
            
            //  Now can walk through picking nodes that either have no dependencies
            //  or all its dependencies have already been picked 
            
            var unordered = new List<string>(nodes.Keys);
            var ordered = new List<string>();
            
            while (unordered.Count > 0)
            {
                var nextID = unordered.FirstOrDefault(nodeID =>
                {
                    return dependencies.Get(nodeID)?.All(input => ordered.Contains(input)) ?? true;
                });

                if (nextID == null)
                    throw new Exception($"Cannot order nodes, cycle in graph?\nOrdered = {ordered.Join(",")}\nUnordered = {unordered.Join(",")}");
                

                ordered.Add(nextID);
                unordered.Remove(nextID);
            }

            return ordered;
        }
    }
}