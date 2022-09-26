using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using OzricService;

namespace OzricUI.Mock;

public class MockEngineService: IEngineService
{
    public EngineStatus Status  => new EngineStatus
    {
        paused = _paused,
        states = Home.GetEntityStates(Graph.GetInterestedEntityIDs())
    };
    
    public Graph Graph { get; private set; }
    
    public Home Home { get; private set; }

    private bool _paused;

    public MockEngineService()
    {
        Home = Json.Deserialize<Home>(File.ReadAllText("Mock/Home.json"));
    }

    public async Task Start(CancellationToken _)
    {
        Graph = await EngineService.LoadGraph();
    }
    
    public void SetPaused(bool statusPaused)
    {
        _paused = statusPaused;
    }

    public async Task Restart(Graph graph)
    {
        await EngineService.SaveGraph(graph);
        Graph = graph;
    }
}