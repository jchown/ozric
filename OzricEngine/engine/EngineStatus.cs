using System.Collections.Generic;

namespace OzricEngine.engine;

public class EngineStatus
{
    public CommsStatus comms;
    public List<EntityState> states;
    public bool paused;
}