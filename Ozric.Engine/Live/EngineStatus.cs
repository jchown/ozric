using System.Collections.Generic;
using Ozric.Engine.Model;

namespace Ozric.Engine.Live;

public class EngineStatus
{
    public CommsStatus comms;
    public List<EntityState> states;
    public bool paused;
}