using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    /// <summary>
    /// Main loop that drives all node behaviour
    /// </summary>
    public class Engine : OzricObject
    {
        public readonly Home home;
        public readonly Comms comms;

        private readonly Dictionary<string, Node> nodes;
        private readonly Dictionary<OutputSelector, List<InputSelector>> edges;

        public Engine(Home home, Comms comms)
        {
            this.home = home;
            this.comms = comms;

            nodes = new Dictionary<string, Node>();
            edges = new Dictionary<OutputSelector, List<InputSelector>>();
        }

        public void AddNode(Node node)
        {
            nodes[node.id] = node;
        }

        public void Connect(OutputSelector output, InputSelector input)
        {
            if (!nodes.ContainsKey(output.nodeID))
                throw new Exception();

            if (!nodes.ContainsKey(input.nodeID))
                throw new Exception();

            edges.GetOrSet(output, () => new List<InputSelector>()).Add(input);

            Log(LogLevel.Debug, "{0}.{1} -> {2}.{3}", output.nodeID, output.outputName, input.nodeID, input.inputName);
        }

        public void Disconnect(OutputSelector output, InputSelector input)
        {
            edges.Get(output).Remove(input);
        }

        public async Task MainLoop(CancellationToken? cancellationToken = null)
        {
            try
            {
                await comms.StartMessagePump(this);

                await InitNodes();

                while (!(cancellationToken?.IsCancellationRequested ?? false))
                {
                    await UpdateNodes();
                    
                    //  Avoid spinning constantly by waiting for an event (that we aren't responsible for)
                    //  OR a period of time has elapsed.
                    
                    DateTime waitTimeout = home.GetTime().AddSeconds(3);

                    while (true)
                    {
                        int millisToWait = (int)(waitTimeout - home.GetTime()).TotalMilliseconds;
                        if (millisToWait <= 0)
                            break;
                        
                        var events = comms.TakePendingEvents(millisToWait);
                        if (events.Count > 0)
                        {
                            if (ProcessEvents(events))
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, "Main loop threw exception: {0}", e);
            }
        }
        
        /// <summary>
        /// Process all the incoming events
        /// </summary>
        /// <param name="events"></param>
        /// <returns>True if any events are unexpected</returns>

        protected bool ProcessEvents(List<ServerEvent> events)
        {
            var unexpected = false;
            
            foreach (var ev in events)
            {
                Log(LogLevel.Trace, "Processing event: {0}", ev);

                switch (ev.payload)
                {
                    case EventStateChanged stateChanged:
                    {
                        unexpected |= ProcessEvent(stateChanged);
                        break;
                    }

                    case EventCallService callService:
                    {
                        Log(LogLevel.Debug, "Event {1}: {2}", callService.data.domain, callService.data.service_data.entity_id.Join(","), callService.data.service);
                        break;
                    }
                }
            }

            return unexpected;
        }

        private bool ProcessEvent(EventStateChanged stateChanged)
        {
            var newState = stateChanged.data.new_state;

            var entity = home.GetEntityState(newState.entity_id);
            if (entity == null)
            {
                Log(LogLevel.Warning, "Unknown entity {0}", newState.entity_id);
                return false;
            }

            var now = home.GetTime();
            var expected = entity.WasRecentlyUpdatedByOzric(now, SELF_EVENT_SECS);
            if (!expected)
            {
                entity.lastUpdatedByOther = now;
                if (!entity.entity_id.Contains("panasonic"))
                    Log(LogLevel.Info, "{0} = {1}", newState.entity_id, stateChanged.data.new_state);
                
                entity.state = newState.state;
                entity.attributes = newState.attributes;
                entity.last_updated = newState.last_updated;
                entity.last_changed = newState.last_changed;
            }
            

            return !expected;
        }

        public async Task InitNodes()
        {
            await ProcessNodes((node, context) => node.OnInit(context));
        }

        public async Task UpdateNodes()
        {
            await ProcessNodes((node, context) => node.OnUpdate(context));
        }

        public interface ICommandSender
        {
            void Add(ClientCommand command, Action<ServerResult> resultHandler);
        }

        /// <summary>
        /// Process all nodes, ordering according to dependencies.
        /// </summary>
        /// <param name="nodeProcessor"></param>
        private async Task ProcessNodes(Func<Node, Context, Task> nodeProcessor)
        {
            var commandSender = new CommandSender();
            var context = new Context(this, commandSender);
            var dependencies = GetNodeDependencies();
            var readiness = new Dictionary<string, SemaphoreSlim>();
            var tasks = new List<Task>();

            foreach (var (nodeID, nodeEdges) in dependencies)
            {
                var numInputs = nodeEdges.inputNodeIDs.Count;
                if (numInputs > 0)
                    readiness[nodeID] = new SemaphoreSlim(0, numInputs);
            }

            foreach (var (nodeID, node) in nodes)
            {
                var semaphore = readiness.GetValueOrDefault(nodeID);
                var dependency = dependencies[nodeID];

                tasks.Add(Task.Run(async () =>
                {
                    if (semaphore != null)
                    {
                        //  Wait for all out dependencies to have signalled us

                        for (int i = 0; i < dependency.inputNodeIDs.Count; ++i)
                            await semaphore.WaitAsync();
                    }

                    try
                    {
                        Log(LogLevel.Trace, "Processing {0}", nodeID);

                        //  Run node lifecycle method

                        await nodeProcessor(node, context);

                        //  Copy outputs to relevant inputs

                        CopyNodeOutputValues(node);
                    }
                    finally
                    {
                        Log(LogLevel.Trace, "Releasing {0} dependents", nodeID);

                        //  Now signal all our dependents

                        foreach (var nodeID in dependency.outputNodeIDs)
                            readiness[nodeID].Release();
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            await commandSender.Send(comms);
        }

        /// <summary>
        /// Copy all output values to connected nodes' inputs
        /// </summary>
        /// <param name="node"></param>
        private void CopyNodeOutputValues(Node node)
        {
            foreach (var output in node.outputs)
            {
                var selector = new OutputSelector { nodeID = node.id, outputName = output.name };
                var value = output.value;

                if (!edges.ContainsKey(selector))
                {
                    Log(LogLevel.Warning, "Missing output selector for {0}.{1}", node.id, output.name);
                    continue;
                }

                foreach (var input in edges[selector])
                {
                    Log(LogLevel.Debug, "{0}.{1} = {2}", input.nodeID, input.inputName, value);

                    nodes[input.nodeID].SetInputValue(input.inputName, value);
                }
            }
        }

        internal class NodeEdges
        {
            internal HashSet<string> inputNodeIDs = new HashSet<string>();
            internal HashSet<string> outputNodeIDs = new HashSet<string>();
        }

        /// <summary>
        /// Return a mapping of nodes -> list of nodes that are dependencies of, and dependent on, that node.
        /// e.g. For a graph of [A -> B, A -> C] return [A -> [][B,C], B -> [A][], C -> [A][] ]
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, NodeEdges> GetNodeDependencies()
        {
            //  Work out the dependencies for each node

            var nodeEdges = new Dictionary<string, NodeEdges>();

            foreach (var (outputSelector, inputSelectors) in edges)
            {
                var fromID = outputSelector.nodeID;
                var fromNodeEdges = nodeEdges.GetOrSet(fromID, () => new NodeEdges());

                foreach (var output in inputSelectors)
                {
                    var toID = output.nodeID;

                    var toNodeEdges = nodeEdges.GetOrSet(toID, () => new NodeEdges());

                    fromNodeEdges.outputNodeIDs.Add(toID);
                    toNodeEdges.inputNodeIDs.Add(fromID);
                }
            }

            return nodeEdges;
        }

        /// <summary>
        /// Process all nodes, one at a time. May be useful when debugging.
        /// </summary>
        /// <param name="nodeProcessor"></param>
        public async Task ProcessNodesSerial(Func<Node, Task> nodeProcessor)
        {
            foreach (var nodeID in GetNodesInOrder())
            {
                var node = nodes[nodeID];

                await nodeProcessor(node);

                CopyNodeOutputValues(node);
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

            //  Some nodes have no edges

            foreach (var node in nodes)
            {
                if (!dependencies.ContainsKey(node.Key))
                    dependencies[node.Key] = new List<string>();
            }

            //  Check no dependencies are missing

            foreach (var dependency in dependencies)
            {
                foreach (var nodeID in dependency.Value)
                {
                    if (!dependencies.ContainsKey(nodeID))
                    {
                        throw new Exception($"Node '{dependency.Key}' depends on '{nodeID}', but is not in graph");
                    }
                }
            }

            //  Now can walk through picking nodes that either have no dependencies
            //  or all its dependencies have already been picked 

            var unordered = new List<string>(nodes.Keys);
            var ordered = new List<string>();

            while (unordered.Count > 0)
            {
                var nextID = unordered.FirstOrDefault(nodeID => { return dependencies.Get(nodeID)?.All(input => ordered.Contains(input)) ?? true; });

                if (nextID == null)
                    throw new Exception($"Cannot order nodes, cycle in graph?\nOrdered = {ordered.Join(",")}\nUnordered = {unordered.Join(",")}");

                ordered.Add(nextID);
                unordered.Remove(nextID);
            }

            return ordered;
        }

        private const int SELF_EVENT_SECS = 30;

        public override string Name => "Engine";
    }
}