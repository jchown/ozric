using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozric.Engine.Graph;
using Ozric.Engine.Utils;
using Ozric.Engine.Nodes;
using Sentry;

namespace Ozric.Engine.Live;

/// <summary>
/// Drives node behaviour over a graph. Stateful in the sense that nodes carry state
/// across updates, but has no IO of its own — events come in via <see cref="ProcessEvents"/>,
/// commands go out via the <see cref="ICommandSink"/> passed to <see cref="UpdateNodes"/>.
/// </summary>
public class Engine : OzricObject
{
    public readonly IHome home;
    public readonly Graph.Graph graph;

    public delegate void StateChangedHandler(EventStateChanged o);
    public event StateChangedHandler? entityStateChanged;

    public event Pin.Changed? pinChanged;
    public event Alert.Changed? alertChanged;

    public bool paused { get; set; }

    public override string Name => "Engine";

    public Engine(IHome home, Graph.Graph graph)
    {
        this.home = home;
        this.graph = graph;
    }

    /// <summary>
    /// Process all the incoming events.
    /// </summary>
    /// <returns>True if any events triggered an external state change</returns>
    public bool ProcessEvents(List<ServerEvent> events)
    {
        var external = false;

        foreach (var ev in events)
        {
            Log(LogLevel.Debug, "Processing event: {0}", ev);

            try
            {
                if (ev.payload is EventStateChanged stateChanged)
                {
                    if (!stateChanged.data.new_state.IsRedacted())
                        Log(LogLevel.Info, "Event - {0}: {1}", stateChanged.data.new_state.entity_id, stateChanged.data.new_state.state);

                    external |= home.OnEventStateChanged(stateChanged);
                    entityStateChanged?.Invoke(stateChanged);
                }
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, "Failed to process event: {0}\nEvent: {1}", e, ev);
                SentrySdk.CaptureException(e);
            }
        }

        return external;
    }

    public Task InitNodes(ICommandSink sink) => ProcessNodes(sink, (node, context) => node.OnInit(context));

    public Task UpdateNodes(ICommandSink sink) => ProcessNodes(sink, (node, context) => node.OnUpdate(context));

    /// <summary>
    /// Process all nodes in dependency order.
    /// </summary>
    public async Task ProcessNodes(ICommandSink sink, Func<GraphNode, Context, Task> nodeProcessor)
    {
        var context = new Context(home, sink, pinChanged, alertChanged);

        foreach (var nodeID in graph.GetNodesInOrder())
        {
            var node = graph.GetNode(nodeID)!;

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
                Log(LogLevel.Error, "Failed to process node {0}: {1}", node.Name, e.Message);
                SentrySdk.CaptureException(e);
            }
        }
    }

    public EngineStatus Status => new()
    {
        states = home.GetEntityStates(graph.GetInterestedEntityIDs()),
        paused = paused
    };
}
