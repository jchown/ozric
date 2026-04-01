using System.Buffers;
using System.Text;
using System.Text.Json;
using Ozric.Engine;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Live;
using Ozric.Engine.Utils;
using Ozric.Service;
using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using Graph = Ozric.Engine.Graph.Graph;

namespace OzricService;

public class OzricService: IOzricService, ICommandSender
{
    private Engine? _engine;
    private IComms? _comms;

    public Engine Engine => _engine ?? throw new InvalidOperationException();
    public EngineStatus Status => _engine?.Status ?? new EngineStatus();
    public Graph Graph => Engine.graph ?? throw new InvalidOperationException();
    public IHome Home => Engine.home ?? throw new InvalidOperationException();
    public ICommandSender CommandSender => this;

    private readonly CancellationTokenSource _mainLoopCancel = new();

    public async Task Start(CancellationToken cancellationToken)
    {
        var graph = await LoadGraph();

        ValidateAndFixGraph(graph);
        
        _comms = await Connect();

        var home = new Home(_comms);

        await home.WaitForEntities();

        CheckAreas(home, graph);
        
        _engine = new Engine(home, graph, _comms);

        var token = _mainLoopCancel.Token;
        Tasks.Run(() => _engine.MainLoop(token), token);
    }


    private static async Task<Graph> LoadGraph()
    {
        Graph graph = new Graph();

        if (File.Exists(Storage.GraphFilename))
        {
            try
            {
                var json = await File.ReadAllTextAsync(Storage.GraphFilename);
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

    private static void ExamineGraph(string json)
    {
        Console.WriteLine("Checking nodes");
        
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
                    Json.Deserialize<Node>(nodeJson);
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

        var edgeChecks = new Func<Edge, string?>[]
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
        await File.WriteAllTextAsync(Storage.GraphFilename, json);
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
            return new Comms(Comms.INGRESS_API, supervisor);
        }

        var core = Environment.GetEnvironmentVariable("CORE_TOKEN");
        if (core != null)
        {
            var wsUri = Comms.CORE_API;
            HaConnectionInfo.BaseHttpUrl = $"http://{wsUri.Host}:{wsUri.Port}";
            HaConnectionInfo.Token = core;
            return new Comms(Comms.CORE_API, core);
        }

        throw new Exception("No tokens");
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        try
        {
            _mainLoopCancel.Cancel();
            
            if (_engine != null)
            {
                _engine.Dispose();
                _engine = null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return Task.CompletedTask;
    }

    public async Task Restart(Graph graph)
    {
        CancellationToken cancellationToken = CancellationToken.None;

        await Stop(cancellationToken);

        if (!graph.Equals(Graph))
        {
            await SaveGraph(graph);
        }

        await Start(cancellationToken);
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
        Engine.pinChanged += pinChanged;
        Engine.alertChanged += alertChanged;
    }
    
    public void Unsubscribe(Pin.Changed pinChanged, Alert.Changed alertChanged)
    {
        Engine.pinChanged -= pinChanged;
        Engine.alertChanged -= alertChanged;
    }
    
    private void CheckAreas(IHome home, Graph graph)
    {
        var entities = home.GetEntityConfigs();
        var devices = home.GetDeviceConfigs();
        
        var toGuess = new List<string>();
        
        //  Nodes (usually entities) that have an obvious area
        
        foreach (var node in graph.nodes.Values)
        {
            if (node is Constant constantNode && constantNode.ConstantType == Ozric.Engine.Values.ValueType.Color)
            {
                node.area_id = IHome.PaletteId;
            }
            else if (string.IsNullOrEmpty(node.area_id))
            {
                if (node is EntityNode entityNode)
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
                    break;
                }
                
                graph.nodes[node].area_id = IHome.GlobalAreaId;
                toGuess.Remove(node);
                Console.WriteLine($"Guessing area of {node} is {IHome.GlobalAreaId}");
                break;
            }
        }
    }
 }