using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OzricEngine.engine;
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
        public readonly Graph graph;
        public readonly Comms comms;

        private bool _paused;
        public bool paused
        {
            get => _paused;
            set
            {
                Log(LogLevel.Warning, "Paused: {0}", value);
                _paused = value;
            }
        }
        
        public Engine(Home home, Graph graph, Comms comms)
        {
            this.home = home;
            this.graph = graph;
            this.comms = comms;
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
                        if (events.Count > 0 && !paused)
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

                try
                {
                    switch (ev.payload)
                    {
                        case EventStateChanged stateChanged:
                        {
                            unexpected |= ProcessEvent(stateChanged);
                            break;
                        }

                        case EventCallService callService:
                        {
                            var entityId = callService.data.service_data.entity_id;
                            var source = entityId != null ? entityId.Join(",") : "<unknown entity>";

                            Log(LogLevel.Debug, "Event {1}: {2} {3}", callService.data.domain, source, callService.data.service);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(LogLevel.Error, "Failed to process event: {0}\nEvent: {1}", e, ev);
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
            
            Log(LogLevel.Info, "{0}: {1}", newState.entity_id, newState.state);

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
            var dependencies = graph.GetNodeDependencies();
            var readiness = new Dictionary<string, SemaphoreSlim>();
            var tasks = new List<Task>();

            foreach (var (nodeID, nodeEdges) in dependencies)
            {
                var numInputs = nodeEdges.inputNodeIDs.Count;
                if (numInputs > 0)
                    readiness[nodeID] = new SemaphoreSlim(0, numInputs);
            }

            foreach (var nodeID in graph.GetNodeIDs())
            {
                var node = graph.GetNode(nodeID);
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

                        graph.CopyNodeOutputValues(node);
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
        /// Process all nodes, one at a time. May be useful when debugging.
        /// </summary>
        /// <param name="nodeProcessor"></param>
        public async Task ProcessNodesSerial(Func<Node, Task> nodeProcessor)
        {
            foreach (var nodeID in graph.GetNodesInOrder())
            {
                var node = graph.GetNode(nodeID);

                await nodeProcessor(node);

                graph.CopyNodeOutputValues(node);
            }
        }

        private const int SELF_EVENT_SECS = 30;

        public override string Name => "Engine";
        
        public EngineStatus Status => new EngineStatus { comms = comms.Status, paused = paused };

        public void Dispose()
        {
        }
    }
}