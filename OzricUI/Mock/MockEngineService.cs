using OzricEngine;
using OzricEngine.engine;
using OzricEngine.logic;
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
        Graph = EngineService.LoadGraph();
        Home = Json.Deserialize<Home>(File.ReadAllText("Mock/Home.json"));
    }
    
    public void SetPaused(bool statusPaused)
    {
        _paused = statusPaused;
    }

    public Task Restart(Graph graph)
    {
        Graph = graph;
        return Task.CompletedTask;
    }
}