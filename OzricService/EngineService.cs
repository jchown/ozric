using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using Graph = OzricEngine.Graph;

namespace OzricService;

public class EngineService: IEngineService, ICommandSender
{
    const string GRAPH_FILENAME = "/data/graph.json";

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
        var json = Json.Prettify(Json.Serialize(graph));
        Console.WriteLine(json);
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

    public async Task<ServerResult> Send(ClientCommand command)
    {
        if (comms == null)
            throw new Exception("Not connected");
        
        return await comms.SendCommand(command, 1000);
    }
}