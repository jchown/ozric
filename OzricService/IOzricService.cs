using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;

namespace OzricService;

/// <summary>
/// Client/server agnostic interface to the engine
/// </summary>
public interface IOzricService
{
    EngineStatus Status { get; }
    
    Graph Graph { get; }
    
    IHome Home { get; }
    
    ICommandSender CommandSender { get; }

    Task Start(CancellationToken token);
    
    void SetPaused(bool statusPaused);
    
    Task Restart(Graph graph);
}