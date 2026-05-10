using Ozric.Engine.Graph;
using Ozric.Engine;
using Ozric.Engine.Live;

namespace Ozric.Service;

/// <summary>
/// Interface to the engine
/// </summary>
public interface IOzricService
{
    public Task AwaitStarted();

    EngineStatus Status { get; }
    
    Graph Graph { get; }
    
    IHome Home { get; }
    
    ICommandSender CommandSender { get; }

    Task Start(CancellationToken token);
    
    void SetPaused(bool statusPaused);

    Task SaveGraph(Graph graph);
    
    Task Restart(Graph graph);
    
    void Subscribe(Pin.Changed pinChanged, Alert.Changed alertChanged);
    
    void Unsubscribe(Pin.Changed pinChanged, Alert.Changed alertChanged);
}