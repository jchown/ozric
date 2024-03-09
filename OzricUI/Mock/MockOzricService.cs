using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using OzricService;

namespace OzricUI.Mock;

public class MockOzricService: IEngineService
{
    public EngineStatus Status  => new EngineStatus
    {
        paused = _paused,
        states = Home.GetEntityStates(Graph.GetInterestedEntityIDs())
    };
    
    public Graph Graph { get; private set; }
    
    public Home Home { get; }

    public ICommandSender CommandSender => throw new InvalidOperationException();
        
    private bool _paused;

    public MockOzricService()
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

    public void Subscribe(Pin.Changed pinChanged, Alert.Changed alertChanged)
    {
    }

    public void Unsubscribe(Pin.Changed pinChanged, Alert.Changed alertChanged)
    {
    }
}