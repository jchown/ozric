using OzricEngine;
using OzricEngine.Nodes;

namespace OzricService;

/// <summary>
/// Server-side only extensions to IOzricService 
/// </summary>
public interface IEngineService: IOzricService
{
    public void Subscribe(Pin.Changed? pinChanged);
}