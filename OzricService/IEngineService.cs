using OzricEngine;
using OzricEngine.engine;
using OzricEngine.Nodes;
using OzricEngine.Values;

namespace OzricService;

/// <summary>
/// Server-side only extensions to IOzricService 
/// </summary>
public interface IEngineService: IOzricService
{
    void Subscribe(Pin.Changed pinChanged, Alert.Changed alertChanged);
    
    void Unsubscribe(Pin.Changed pinChanged, Alert.Changed alertChanged);
}