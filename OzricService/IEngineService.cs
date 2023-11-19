using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;

namespace OzricService;

/// <summary>
/// Server-side only extensions to IOzricService 
/// </summary>
public interface IEngineService: IOzricService
{
    void Subscribe(Pin.Changed pinChanged);
    
    void Unsubscribe(Pin.Changed pinChanged);
}