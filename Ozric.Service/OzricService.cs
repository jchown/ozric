using System.Buffers;
using System.Text;
using System.Text.Json;
using Ozric.Engine;
using Ozric.Engine.Extensions;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Live;
using Ozric.Engine.Messages;
using Ozric.Engine.Nodes;
using Ozric.Engine.Utils;
using Sentry;

namespace Ozric.Service;

using Engine = Ozric.Engine.Live.Engine;
using Graph = Ozric.Engine.Graph.Graph;

public class OzricService: OzricObject, IOzricService, ICommandSender
{
    public override string Name => "OzricService";

    private Engine? _engine;
    private IComms? _comms;
    private CommandBatcher? _batcher;

    private readonly Heartbeat _mainLoopHeartbeat = new();

    /// <summary>
    /// Where the graph is persisted
    /// </summary>
    private readonly string _graphFilename;

    /// <summary>
    /// Each engine gets its own main loop, so each needs its own cancellation (a CTS can't be un-cancelled).
    /// </summary>
    private CancellationTokenSource? _mainLoopCancel;
    private Task? _mainLoopTask;

    /// <summary>
    /// Subscribers are held here, not on the engine, so they survive the engine being replaced.
    /// </summary>
    private Pin.Changed? _pinChanged;
    private Alert.Changed? _alertChanged;

    /// <summary>
    /// Only one thing at a time may replace the running engine.
    /// </summary>
    private readonly SemaphoreSlim _engineSwap = new(1, 1);

    public OzricService(string? graphFilename = null)
    {
        _graphFilename = graphFilename ?? Storage.GraphFilename;
    }

    public Engine Engine => _engine ?? throw new InvalidOperationException();

    public EngineStatus Status
    {
        get
        {
            if (_engine == null)
                return new EngineStatus();

            var status = _engine.Status;
            if (_comms != null)
                status.comms = _comms.Status;

            status.mainLoop = _mainLoopHeartbeat.Snapshot();

            return status;
        }
    }

    public Graph Graph => Engine.graph ?? throw new InvalidOperationException();
    public IHome Home => Engine.home ?? throw new InvalidOperationException();
    public ICommandSender CommandSender => this;

    public async Task Start(CancellationToken cancellationToken)
    {
        Log(LogLevel.Info,"Starting");

        var graph = await LoadGraph();

        ValidateAndFixGraph(graph);

        Log(LogLevel.Info,"Connecting");

        var comms = _comms = await Connect();
        comms.OnSentMessage(OnSendUpdateEntityUpdateTime);

        Log(LogLevel.Info,"Creating Home");

        var home = new Home(comms);

        await home.WaitForEntities();

        Run(home, comms, graph);
    }

    /// <summary>
    /// Run a graph against a home, over an established connection.
    /// </summary>
    internal void Run(IHome home, IComms comms, Graph graph)
    {
        _comms = comms;

        CheckAreas(home, graph);

        StartEngine(home, comms, graph, paused: false);
    }

    /// <summary>
    /// Create an engine for the graph and start its main loop. The caller must have stopped any previous main loop.
    /// </summary>
    private void StartEngine(IHome home, IComms comms, Graph graph, bool paused)
    {
        Log(LogLevel.Info, "Creating Engine");

        var engine = new Engine(home, graph) { paused = paused };
        engine.pinChanged += (nodeID, pinName, value) => _pinChanged?.Invoke(nodeID, pinName, value);
        engine.alertChanged += nodeID => _alertChanged?.Invoke(nodeID);

        Log(LogLevel.Info, "Creating Command Batcher");

        var batcher = new CommandBatcher();
        var cancel = new CancellationTokenSource();

        _engine = engine;
        _batcher = batcher;
        _mainLoopCancel = cancel;
        _mainLoopTask = Task.Run(async () =>
        {
            try
            {
                await MainLoop(engine, batcher, comms, cancel.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in main loop: {e.Message}: {e.StackTrace}");
                SentrySdk.CaptureException(e);
            }
        });
    }

    /// <summary>
    /// Stop the main loop and wait for it to finish, so nothing is still using the engine afterwards.
    /// </summary>
    private async Task StopMainLoop()
    {
        var cancel = _mainLoopCancel;
        var task = _mainLoopTask;
        _mainLoopCancel = null;
        _mainLoopTask = null;

        if (cancel == null)
            return;

        cancel.Cancel();

        if (task != null)
            await task;

        cancel.Dispose();
    }

    public async Task AwaitStarted()
    {
        while (_engine == null)
        {
            await Task.Yield();
        }
    }

    private async Task MainLoop(Engine engine, CommandBatcher batcher, IComms comms, CancellationToken cancellationToken)
    {
        Log(LogLevel.Info,"Running Main Loop");

        var home = engine.home;

        try
        {
            await engine.InitNodes(batcher);
            await Flush(batcher, comms);

            while (!cancellationToken.IsCancellationRequested)
            {
                _mainLoopHeartbeat.Iterated();

                if (!engine.paused)
                {
                    try
                    {
                        try
                        {
                            await engine.UpdateNodes(batcher);
                        }
                        catch (Exception e)
                        {
                            throw e.Rethrown("While updating nodes");
                        }

                        try
                        {
                            await Flush(batcher, comms);
                        }
                        catch (Exception e)
                        {
                            throw e.Rethrown("While flushing comms");
                        }
                    }
                    catch (Exception e)
                    {
                        _mainLoopHeartbeat.Threw(e);
                        Console.WriteLine($"Main loop threw exception: {e}");
                        SentrySdk.CaptureException(e);
                    }
                }

                //  Avoid spinning constantly by waiting for an event (that we aren't responsible for)
                //  OR a period of time has elapsed.

                var waitTimeout = home.GetTime().AddMilliseconds(100);

                while (true)
                {
                    var millisToWait = (int) (waitTimeout - home.GetTime()).TotalMilliseconds;
                    if (millisToWait <= 0)
                        break;

                    var events = comms.TakePendingEvents(millisToWait);
                    if (events.Count == 0)
                        continue;
                    
                    if (engine.ProcessEvents(events))
                        break;
                }
            }
        }
        finally
        {
            Log(LogLevel.Info,"Main loop ended");
        }
    }

    private static async Task Flush(CommandBatcher batcher, IComms comms)
    {
        if (batcher.commands.Count == 0)
            return;

        await batcher.Send(comms);
    }

    /// <summary>
    /// Housekeeping to avoid spamming entities — record when we last sent a service call for them.
    /// </summary>
    private void OnSendUpdateEntityUpdateTime(object message)
    {
        if (_engine == null)
            return;

        if (message is not ClientCallService ccs)
            return;
        
        foreach (var entityID in ccs.GetEntities())
            _engine.home.SetUpdatedTime(entityID);
    }


    private async Task<Graph> LoadGraph()
    {
        Graph graph = new Graph();

        if (File.Exists(_graphFilename))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_graphFilename);
                try
                {
                    graph = Graph.Deserialize(json);

                    Console.WriteLine($"Loaded graph with {graph.nodes.Count} nodes and {graph.edges.Count} edges");

                    return graph;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load graph: {e.Message}");
                    SentrySdk.CaptureException(e);

                    ExamineGraph(json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load graph: {e.Message}");
            }
        }

        Console.WriteLine($"Starting with empty graph");
        return graph;
    }

    private void ExamineGraph(string json)
    {
        Log(LogLevel.Info, "Checking nodes");
        
        var bytes = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(json));
        var options = new JsonReaderOptions();
        var reader = new Utf8JsonReader(bytes, options);
        
        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            var nodes = jsonDocument.RootElement.GetProperty("nodes");
            foreach (var node in nodes.EnumerateObject())
            {
                try
                {
                    var nodeJson = node.Value.ToString();
                    Json.Deserialize<GraphNode>(nodeJson);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load node {node.Name}: {e.Message}\nValue was {node.Value}");
                    SentrySdk.CaptureMessage($"Failed to load node {node.Name}: {e.Message}\nValue was {node.Value}");
                }
            }
        }
    }

    private void ValidateAndFixGraph(Graph graph)
    {
        //  Check that all references are valid

        var edgeChecks = new Func<GraphEdge, string?>[]
        {
            edge => graph.nodes.ContainsKey(edge.from.nodeID) ? null : $"The from node '{edge.from.nodeID}' does not exist",
            edge => graph.nodes.ContainsKey(edge.to.nodeID) ? null : $"The to node '{edge.to.nodeID}' does not exist",
            edge => graph.nodes[edge.from.nodeID].HasOutput(edge.from.outputName) ? null : $"The from output '{edge.from.outputName}' does not exist on node '{edge.from.nodeID}'",
            edge => graph.nodes[edge.to.nodeID].HasInput(edge.to.inputName) ? null : $"The to input '{edge.to.inputName}' does not exist on node '{edge.to.nodeID}'"
        };
        
        var badEdges = new List<string>();

        foreach (var edge in graph.edges.Values)
        {
            var error = edgeChecks.Select(c => c(edge)).FirstOrDefault();
            if (error == null)
            {
                continue;
            }

            Console.WriteLine($"Removing edge '{edge}' - {error}");
            badEdges.Add(edge.id);
        }
        
        foreach (var edgeId in badEdges)
        {
            graph.edges.Remove(edgeId);
        }
    }

    public async Task SaveGraph(Graph graph)
    {
        var json = Json.Prettify(Json.Serialize(graph));
        Console.WriteLine(json);
        await File.WriteAllTextAsync(_graphFilename, json);
    }

    /// <summary>
    /// Persist an edited graph, then swap the engine over to it.
    /// </summary>
    /// <remarks>
    /// The engine runs its own copy, rebuilt from exactly what was written to disk (and checked
    /// the same way as on startup), so what you see after a reload is what you'd get after a restart.
    /// The comms connection and home are kept, so this is quick and doesn't disturb Home Assistant.
    /// </remarks>
    public async Task ApplyGraph(Graph graph)
    {
        await _engineSwap.WaitAsync();
        try
        {
            if (_engine == null || _comms == null)
                throw new InvalidOperationException("The service is not running");

            Log(LogLevel.Info, "Applying graph with {0} nodes and {1} edges", graph.nodes.Count, graph.edges.Count);

            var json = Json.Prettify(Json.Serialize(graph));
            await File.WriteAllTextAsync(_graphFilename, json);

            var home = _engine.home;
            var paused = _engine.paused;

            var live = Graph.Deserialize(json);
            ValidateAndFixGraph(live);
            CheckAreas(home, live);

            await StopMainLoop();
            StartEngine(home, _comms, live, paused);
        }
        finally
        {
            _engineSwap.Release();
        }
    }

    private static async Task<IComms> Connect()
    {
        var comms = CreateComms();
        await comms.Connect();
        return comms;
    }

    private static Comms CreateComms()
    {
        var supervisor = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
        if (supervisor != null)
        {
            HaConnectionInfo.BaseHttpUrl = "http://supervisor/core";
            HaConnectionInfo.Token = supervisor;
            return new Comms(Comms.IngressApi, supervisor);
        }

        var core = Environment.GetEnvironmentVariable("CORE_TOKEN");
        if (core != null)
        {
            //  Dev override: lets a docker-compose stack point at an in-network HA
            //  instance instead of the hardcoded LAN IP in Comms.CoreApi.
            var wsUriOverride = Environment.GetEnvironmentVariable("OZRIC_HA_WS_URL");
            var wsUri = wsUriOverride != null ? new Uri(wsUriOverride) : Comms.CoreApi;
            HaConnectionInfo.BaseHttpUrl = $"http://{wsUri.Host}:{wsUri.Port}";
            HaConnectionInfo.Token = core;
            return new Comms(wsUri, core);
        }

        throw new Exception("No tokens");
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        try
        {
            await StopMainLoop();

            _comms?.Dispose();
            _comms = null;
            _engine = null;
            _batcher = null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task Restart(Graph graph)
    {
        await _engineSwap.WaitAsync();
        try
        {
            Log(LogLevel.Info, "Restarting");

            CancellationToken cancellationToken = CancellationToken.None;

            await Stop(cancellationToken);

            await SaveGraph(graph);

            await Start(cancellationToken);
        }
        finally
        {
            _engineSwap.Release();
        }
    }

    public void SetPaused(bool paused)
    {
        if (_engine != null && _engine.paused != paused)
        {
            _engine.paused = paused;
        }
    }

    public async Task<ServerResult> Send(ClientCommand command)
    {
        if (_comms == null)
            throw new Exception("Not connected");
        
        return await _comms.SendCommand<ServerResult>(command, 1000);
    }

    public void Subscribe(Pin.Changed pinChanged, Alert.Changed alertChanged)
    {
        _pinChanged += pinChanged;
        _alertChanged += alertChanged;
    }

    public void Unsubscribe(Pin.Changed pinChanged, Alert.Changed alertChanged)
    {
        _pinChanged -= pinChanged;
        _alertChanged -= alertChanged;
    }
    
    private void CheckAreas(IHome home, Graph graph)
    {
        Log(LogLevel.Info,"Checking Areas");
        
        var entities = home.GetEntityConfigs();
        var devices = home.GetDeviceConfigs();
        
        var toGuess = new List<string>();
        
        //  Nodes (usually entities) that have an obvious area
        
        foreach (var node in graph.nodes.Values)
        {
            if (node is GraphConstant constantNode && constantNode.ConstantType == Ozric.Engine.Values.ValueType.Color)
            {
                node.area_id = IHome.PaletteId;
            }
            else if (string.IsNullOrEmpty(node.area_id))
            {
                if (node is GraphEntity entityNode)
                {
                    var entity = entities.FirstOrDefault(e => e.entity_id == entityNode.entityID);
                    if (entity == null)
                    {
                        Console.WriteLine($"Entity {entityNode.entityID} not found for {node.id}");
                        continue;
                    }
                    
                    var areaId = entity.area_id;
                    if (string.IsNullOrEmpty(areaId))
                    {
                        if (!string.IsNullOrEmpty(entity.device_id))
                        {
                            var device = devices.First(d => d.id == entity.device_id);
                            areaId = device.area_id;
                        }

                        if (string.IsNullOrEmpty(areaId))
                        {
                            Console.WriteLine($"Area of {entity.entity_id} ({entity.id}) not found");
                            areaId = IHome.GlobalAreaId;
                        }
                    }

                    node.area_id = areaId;
                }
                else
                {
                    toGuess.Add(node.id);
                }
            }
        }

        while (toGuess.Count > 0)
        {
            var guessed = false;

            foreach (var node in toGuess)
            {
                var connections = graph.GetConnectedNodes(node);
                var areas = connections.Select(n => n.area_id).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

                if (areas.Count == 0)
                {
                    continue;
                }

                if (areas.Count == 1)
                {
                    var areaId = areas.First();
                    graph.nodes[node].area_id = areaId;
                    toGuess.Remove(node);
                    Console.WriteLine($"Guessing area of {node} is {areaId}");
                    guessed = true;
                    break;
                }

                graph.nodes[node].area_id = IHome.GlobalAreaId;
                toGuess.Remove(node);
                Console.WriteLine($"Guessing area of {node} is {IHome.GlobalAreaId}");
                guessed = true;
                break;
            }

            if (guessed)
                continue;

            //  Nothing left is connected to anything with an area (e.g. a node that has just
            //  been added and not yet wired up), so there's nothing to go on. Don't spin forever.

            foreach (var node in toGuess)
            {
                graph.nodes[node].area_id = IHome.GlobalAreaId;
                Console.WriteLine($"Guessing area of {node} is {IHome.GlobalAreaId} (no connections to go on)");
            }

            break;
        }
    }
}