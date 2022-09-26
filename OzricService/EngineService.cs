using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using Graph = OzricEngine.Graph;

namespace OzricService;

public class EngineService: IEngineService
{
    const string GRAPH_FILENAME = "/data/graph.json";

    private Engine? engine;
    private Task? mainLoop;

    public EngineStatus Status => engine?.Status ?? new EngineStatus();
    public Graph Graph => engine?.graph ?? throw new InvalidOperationException();
    public Home Home => engine?.home ?? throw new InvalidOperationException();

    public async Task Start(CancellationToken cancellationToken)
    {
        var graph = await LoadGraph();

        var connection = Connect();

        await connection.Authenticate();

        await connection.Send(new ClientGetStates());

        var states = await connection.Receive<ServerGetStates>() ?? throw new InvalidOperationException();

        var home = new Home(states.result);

        engine = new Engine(home, graph, connection);

        mainLoop = Task.Run(() => engine.MainLoop());
    }

    public static async Task<Graph> LoadGraph()
    {
        Graph graph = new Graph();

        try
        {
            var json = await File.ReadAllTextAsync(GRAPH_FILENAME);
            graph = Json.Deserialize<Graph>(json);

            Console.WriteLine($"Loaded graph with {graph.nodes.Count} nodes and {graph.edges.Count} edges");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load graph: {e.Message}");
        }

        return graph;
    }


    public static async Task SaveGraph(Graph graph)
    {
        var json = Json.Serialize(graph);
        Console.WriteLine(Json.Prettify(json));
        await File.WriteAllTextAsync(GRAPH_FILENAME, json);
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
}