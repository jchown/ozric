using Ozric.Engine.Graph;
using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;

namespace OzricService;

/// <summary>
/// Interface to the engine
/// </summary>
public interface IOzricService
{
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