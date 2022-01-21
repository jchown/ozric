using OzricEngine;
using OzricEngine.engine;
using OzricEngine.logic;
using OzricService.Model;
using Graph = OzricEngine.Graph;

namespace OzricService;

public class EngineService
{
    const string GRAPH_FILENAME = "/data/graph.json";

    private Engine engine;
    private Task mainLoop;

    public EngineStatus Status => engine.Status;
    public Graph Graph => engine.graph;

    public EngineService()
    {
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        Graph graph = new Graph();

        try
        {
            if (File.Exists(GRAPH_FILENAME))
            {
                var json = File.ReadAllText(GRAPH_FILENAME);
                graph = Json.Deserialize<Graph>(json);
                
                Console.WriteLine($"Loaded graph with {graph.nodes.Count} nodes and {graph.edges.Count}");
            }
            else
            {
                Console.WriteLine("No graph files exists");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load graph: {e.Message}");
        }

        using (var connection = new Comms(Options.Instance.token))
        {
            await connection.Authenticate();

            await connection.Send(new ClientGetStates());

            var states = await connection.Receive<ServerGetStates>();

            var home = new Home(states.result);

            engine = new Engine(home, graph, connection);

            mainLoop = Task.Run(() => engine.MainLoop());
        }
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
}