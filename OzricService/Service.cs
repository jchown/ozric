using OzricEngine;
using OzricEngine.engine;
using OzricEngine.logic;
using OzricService.Model;
using Graph = OzricEngine.Graph;

namespace OzricService;

public class Service
{
    public static Service Instance { get; }

    const string GRAPH_FILENAME = "/data/graph.json";

    private Engine? engine;
    private Task? mainLoop;

    public EngineStatus? Status => engine?.Status;
    public Graph? Graph => engine?.graph;

    static Service()
    {
        Instance = new Service();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        Graph graph = new Graph();

        try
        {
            var json = File.ReadAllText(GRAPH_FILENAME);
            graph = Json.Deserialize<Graph>(json);
            
            Console.WriteLine($"Loaded graph with {graph.nodes.Count} nodes and {graph.edges.Count}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load graph: {e.Message}");
        }

        var connection = Connect();

        await connection.Authenticate();

        await connection.Send(new ClientGetStates());

        var states = await connection.Receive<ServerGetStates>();

        var home = new Home(states.result);

        engine = new Engine(home, graph, connection);

        mainLoop = Task.Run(() => engine.MainLoop());
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

        var json = Json.Serialize(graph);
        File.WriteAllText(GRAPH_FILENAME, json);

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