using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OzricEngine.engine;
using OzricEngine.Nodes;
using OzricEngine.Values;

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
        public readonly CommandBatcher commandBatcher;

        public delegate void StateChangedHandler(EventStateChanged o);
        public event StateChangedHandler? entityStateChanged;
        
        public event Pin.Changed? pinChanged;

        private bool _paused;
        private bool _serial = true;
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
            
            commandBatcher = new CommandBatcher();
            comms.OnSentMessage(OnSendUpdateEntityUpdateTime);
        }
        
        /// <summary>
        /// Housekeeping to avoid spamming entities
        /// </summary>
        /// <param name="message"></param>

        private void OnSendUpdateEntityUpdateTime(object message)
        {
            switch (message)
            {
                case ClientCallService ccs:
                {
                    foreach (var entityID in ccs.GetEntities())
                        home.SetUpdatedTime(entityID);
                    
                    break;
                }
            }
        }

        public async Task MainLoop(CancellationToken? cancellationToken = null)
        {
            try
            {
                await comms.StartMessagePump(this);

                await InitNodes();

                while (!(cancellationToken?.IsCancellationRequested ?? false))
                {
                    if (!paused)
                    {
                        await UpdateNodes();
                    }

                    //  Avoid spinning constantly by waiting for an event (that we aren't responsible for)
                    //  OR a period of time has elapsed.

                    DateTime waitTimeout = home.GetTime().AddMilliseconds(100);

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
                Log(LogLevel.Debug, "Processing event: {0}", ev);

                if (IGNORE_OWN_STATE_CHANGES)
                {
                    var contextID = ev.payload.context.id;
                    if (originatedContexts.Any(oc => oc.callService.context.id == contextID))
                    {
                        Log(LogLevel.Info, "Originated locally, ignored");
                        continue;
                    }
                }

                try
                {
                    switch (ev.payload)
                    {
                        case EventStateChanged stateChanged:
                        {
                            external |= ProcessEventStateChanged(stateChanged);
                            entityStateChanged?.Invoke(stateChanged);
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
        /// One tactic to "the color we set is not the color the light uses" is to ignore the actual
        /// state that gets sent back and assume it did what we asked. This doesn't work if the
        /// setting mechanism isn't robust and we need to retry.
        /// </summary>
        internal const bool IGNORE_OWN_STATE_CHANGES = false;

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
                if (IGNORE_OWN_STATE_CHANGES && sentCommands[i].command is ClientCallService ccs)
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
            //  TODO: if this is a new entity ID, re-sync Home
            
            if (!stateChanged.data.new_state.IsRedacted())
                Log(LogLevel.Info, "Event - {0}: {1}", stateChanged.data.new_state.entity_id, stateChanged.data.new_state.state);

            return home.OnEventStateChanged(stateChanged);
        }

        public async Task InitNodes()
        {
            if (_serial)
                await ProcessNodesSerial((node, context) => node.OnInit(context));
            else
                await ProcessNodesParallel((node, context) => node.OnInit(context));
        }

        public async Task UpdateNodes()
        {
            if (_serial)
                await ProcessNodesSerial((node, context) => node.OnUpdate(context));
            else
                await ProcessNodesParallel((node, context) => node.OnUpdate(context));
        }

        /// <summary>
        /// Process all nodes, ordering according to dependencies.
        /// </summary>
        /// <param name="nodeProcessor"></param>
        private async Task ProcessNodesParallel(Func<Node, Context, Task> nodeProcessor)
        {
            var context = new Context(home, commandBatcher, pinChanged);
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

                        if (node.IsReady())
                        {
                            //  Run node lifecycle method

                            await nodeProcessor(node, context);

                            //  Copy outputs to relevant inputs

                            graph.CopyNodeOutputValues(node, context);
                        }
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

            await SendCommands();
        }

        /// <summary>
        /// Process all nodes, one at a time. May be useful when debugging.
        /// </summary>
        /// <param name="nodeProcessor"></param>
        public async Task ProcessNodesSerial(Func<Node, Context, Task> nodeProcessor)
        {
            var context = new Context(home, commandBatcher, pinChanged);

            foreach (var nodeID in graph.GetNodesInOrder())
            {
                var node = graph.GetNode(nodeID);

                try
                {
                    if (node.IsReady())
                    {
                        await nodeProcessor(node, context);

                        graph.CopyNodeOutputValues(node, context);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    Log(LogLevel.Error, "Failed to process node {0}: {1}", node.Name, e.Message);
                }
            }

            await SendCommands();
        }

        private async Task SendCommands()
        {
            if (commandBatcher.commands.Count == 0)
                return;
            
            //  Record the commands we are about to send, so we can match the response and grab the context ID 

            var dateTime = home.GetTime();
            foreach (var command in commandBatcher.commands)
                sentCommands.Add(new SentCommand(dateTime, command));

            await commandBatcher.Send(comms);
        }

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