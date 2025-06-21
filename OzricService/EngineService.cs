using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Ozric.Engine;
using Ozric.Engine.Graph;
using Ozric.Engine.Live;
using Ozric.Engine.Utils;
using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using Sentry;
using Graph = Ozric.Engine.Graph.Graph;

namespace OzricService;

public class EngineService: IEngineService, ICommandSender
{
    //  Inside the container store everything in /data, outside use ~/.ozric/data
    public static readonly string RootPath = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.ozric/data" : "/data";

    public static readonly string GraphFilename = RootPath + "/graph.json";

    private Engine? _engine;
    private IComms? _comms;

    //  Server API
    public Engine Engine => _engine ?? throw new InvalidOperationException();

    //  Client API
    public EngineStatus Status => _engine?.Status ?? new EngineStatus();
    public Graph Graph => Engine.graph ?? throw new InvalidOperationException();
    public IHome Home => Engine.home ?? throw new InvalidOperationException();
    public ICommandSender CommandSender => this;

    private readonly CancellationTokenSource _mainLoopCancel = new();

    public async Task Start(CancellationToken cancellationToken)
    {
        var graph = await LoadGraph();

        _comms = await Connect();

        var home = new Home(_comms);

        await home.WaitForEntities();
        
        _engine = new Engine(home, graph, _comms);

        var token = _mainLoopCancel.Token;
        Tasks.Run(() => _engine.MainLoop(token), token);
    }

    public static async Task<Graph> LoadGraph()
    {
        Graph graph = new Graph();

        if (File.Exists(GraphFilename))
        {
            try
            {
                var json = await File.ReadAllTextAsync(GraphFilename);
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


    public static async Task SaveGraph(Graph graph)
    {
        var json = Json.Prettify(Json.Serialize(graph));
        Console.WriteLine(json);
        await File.WriteAllTextAsync(GraphFilename, json);
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
            return new Comms(Comms.INGRESS_API, supervisor);
        }
        
        var core = Environment.GetEnvironmentVariable("CORE_TOKEN");
        if (core != null)
        {
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
 }