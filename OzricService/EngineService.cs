using System.Buffers;
using System.Text;
using System.Text.Json;
using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using Graph = OzricEngine.Graph;

namespace OzricService;

public class EngineService: IEngineService, ICommandSender
{
    const string GraphFilename = "/data/graph.json";

    private Engine? engine;
    private Task? mainLoop;
    private Comms? comms;

    //  Server API
    public Engine Engine => engine ?? throw new InvalidOperationException();

    //  Client API
    public EngineStatus Status => engine?.Status ?? new EngineStatus();
    public Graph Graph => Engine.graph ?? throw new InvalidOperationException();
    public Home Home => Engine.home ?? throw new InvalidOperationException();
    public ICommandSender CommandSender => this;

    public async Task Start(CancellationToken cancellationToken)
    {
        var graph = await LoadGraph();

        comms = Connect();

        await comms.Authenticate();

        await comms.Send(new ClientGetStates());

        var states = await comms.Receive<ServerGetStates>() ?? throw new InvalidOperationException();

        var home = new Home(states.result);

        engine = new Engine(home, graph, comms);

        mainLoop = Task.Run(() => engine.MainLoop());
    }

    public static async Task<Graph> LoadGraph()
    {
        Graph graph = new Graph();

        try
        {
            var json = await File.ReadAllTextAsync(GraphFilename);
            try
            {
                //  TODO: Remove post v0.8
                
                json = json.Replace("boolean", "binary");
                json = json.Replace("Boolean", "Binary");
                json = json.Replace("scalar", "number");
                json = json.Replace("Scalar", "Number");
                
                graph = Json.Deserialize<Graph>(json);

                Console.WriteLine($"Loaded graph with {graph.nodes.Count} nodes and {graph.edges.Count} edges");

                return graph;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load graph: {e.Message}");

                ExamineGraph(json);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load graph: {e.Message}");
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

    private static Comms Connect()
    {
        var supervisor = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
        if (supervisor != null)
        {
            var connection = new Comms(Comms.INGRESS_API, supervisor);
            return connection;
        }
        
        var core = Environment.GetEnvironmentVariable("CORE_TOKEN");
        if (core != null)
        {
            var connection = new Comms(Comms.CORE_API, core);
            return connection;
        }
        
        throw new Exception("No tokens");
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        try
        {
            if (mainLoop != null)
            {
                mainLoop.Dispose();
                mainLoop = null;
            }

            if (engine != null)
            {
                engine.Dispose();
                engine = null;
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

        await SaveGraph(graph);

        await Start(cancellationToken);
    }

    public void SetPaused(bool paused)
    {
        if (engine != null && engine.paused != paused)
        {
            engine.paused = paused;
        }
    }

    public async Task<ServerResult> Send(ClientCommand command)
    {
        if (comms == null)
            throw new Exception("Not connected");
        
        return await comms.SendCommand(command, 1000);
    }
    
    public void Subscribe(Pin.Changed? pinChanged)
    {
        Engine.pinChanged += pinChanged;
    }
}