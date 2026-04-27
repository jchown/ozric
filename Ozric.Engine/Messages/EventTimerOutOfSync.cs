using Ozric.Engine.Messages;

namespace Ozric.Engine;

[TypeKey("timer_out_of_sync")]
public class EventTimerOutOfSync: Event
{
    public Attributes data { get; set; }
}