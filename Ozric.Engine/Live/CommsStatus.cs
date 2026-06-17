using Ozric.Engine.Live;

namespace Ozric.Engine;

public class CommsStatus
{
    public bool messagePump { get; set; }

    public HeartbeatStatus? heartbeat { get; set; }
}
