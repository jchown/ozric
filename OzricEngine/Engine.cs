using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OzricEngine.engine;
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
        private readonly List<SentCommand> sentCommands = new();
        private readonly List<OriginatedContext> originatedContexts = new();

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
            finally
            {
                Log(LogLevel.Info, "Main loop ended");
            }
        }
        
        /// <summary>
        /// Process all the incoming events
        /// </summary>
        /// <param name="events"></param>
        /// <returns>True if any events are external</returns>

        protected bool ProcessEvents(List<ServerEvent> events)
        {
            var external = false;
            
            foreach (var ev in events)
            {
                Log(LogLevel.Trace, "Processing event: {0}", ev);

                var contextID = ev.payload.context.id;
                if (originatedContexts.Any(oc => oc.callService.context.id == contextID))
                {
                    Log(LogLevel.Trace, "Originated locally, ignored");
                    continue;
                }
                    
                try
                {
                    switch (ev.payload)
                    {
                        case EventStateChanged stateChanged:
                        {
                            external |= ProcessEventStateChanged(stateChanged);
                            break;
                        }

                        case EventCallService callService:
                        {
                            ProcessEventCallService(callService);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(LogLevel.Error, "Failed to process event: {0}\nEvent: {1}", e, ev);
                }
            }
            
            var now = home.GetTime();
            sentCommands.RemoveAll(sc => sc.Expired(now));
            originatedContexts.RemoveAll(oc => oc.Expired(now));

            return external;
        }

        /// <summary>
        /// Process a service call. Tries to find if it is due to one of our command sends. 
        /// </summary>
        /// <param name="events"></param>
        /// <returns>True if any events are external</returns>

        private void ProcessEventCallService(EventCallService callService)
        {
            var now = home.GetTime();

            for (int i = 0; i < sentCommands.Count;)
            {
                if (sentCommands[i].command is ClientCallService ccs)
                {
                    if (callService.data.OriginatedBy(ccs))
                    {
                        //  This is a call service that we made. Record the context ID
                        
                        sentCommands.RemoveAt(i);
                        originatedContexts.Add(new OriginatedContext(now, callService));
                        continue;
                    }
                }

                i++;
            }
            
            //  Not one of ours
            
            var entityId = callService.data.service_data["entity_id"];
            var source = entityId != null ? entityId/*.Join(",")*/ : "<unknown entity>";

            Log(LogLevel.Debug, "Event {1}: {2} {3}", callService.data.domain, source, callService.data.service);
        }

        /// <summary>
        /// Process a state change event, return true if it is external (we weren't responsible).
        /// </summary>
        /// <param name="events"></param>
        /// <returns>True if the event was external</returns>

        private bool ProcessEventStateChanged(EventStateChanged stateChanged)
        {
            var newState = stateChanged.data.new_state;

            var entityState = home.GetEntityState(newState.entity_id);
            if (entityState == null)
            {
                Log(LogLevel.Warning, "Unknown entity {0}", newState.entity_id);
                return false;
            }

            if (newState.state == "unavailable")
            {
                Log(LogLevel.Debug, "Entity {0}: {1}, ignoring", newState.entity_id, newState.state);
                return false;
            }

            if (entityState.entity_id.StartsWith("light."))
            {
                //  Check only the relevant details, ignoring timers etc.

                if (entityState.state == newState.state && entityState.attributes.EqualsKeys(newState.attributes, Light.ATTRIBUTE_KEYS))
                {
                    Log(LogLevel.Debug, "Entity {0}: unchanged, ignoring", newState.entity_id);
                    return false;
                }
            }

            var now = home.GetTime();
            var expected = entityState.WasRecentlyUpdatedByOzric(now, SELF_EVENT_SECS);
            if (!expected)
            {
                lock (entityState)
                {
                    entityState.lastUpdatedByOther = now;
                    entityState.state = newState.state;
                    entityState.attributes = newState.attributes;
                    entityState.last_updated = newState.last_updated;
                    entityState.last_changed = newState.last_changed;

                    if (entityState.entity_id.StartsWith("light."))
                        entityState.LogLightState();
                }
            }
            else
            {
                Log(LogLevel.Info, "Expected: {0}: {1}", newState.entity_id, newState.state);
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

            if (commandSender.commands.Count > 0)
            {
                await SendCommands(commandSender);
            }
        }

        public async Task SendCommands(CommandSender commandSender)
        {
            //  Record the commands we are about to send, so we can match the response and grab the context ID 

            var dateTime = home.GetTime();
            foreach (var command in commandSender.commands)
                sentCommands.Add(new SentCommand(dateTime, command));

            //  Fire them off and wait for results

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

        public const int SELF_EVENT_SECS = 30;

        public override string Name => "Engine";
        
        public EngineStatus Status => new EngineStatus
        {
            comms = comms.Status,
            states = home.GetEntityStates(graph.GetInterestedEntityIDs()),
            paused = paused
        };

        public void Dispose()
        {
            comms.Dispose();
        }
    }
}