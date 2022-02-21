using System.Collections.Generic;

namespace OzricEngine.engine;

public class EngineStatus
{
    public CommsStatus comms { get; set; }
    public List<EntityState> states { get; set; }
    public bool paused { get; set; }
}