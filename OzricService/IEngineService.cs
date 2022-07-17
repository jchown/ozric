using OzricEngine;
using OzricEngine.engine;
using OzricEngine.logic;

namespace OzricService;

public interface IEngineService
{
    EngineStatus Status { get; }
    
    Graph Graph { get; }
    
    Home Home { get; }
    
    void SetPaused(bool statusPaused);
    
    Task Restart(Graph graph);
}