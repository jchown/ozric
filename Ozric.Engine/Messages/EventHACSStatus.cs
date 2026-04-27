using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("hacs/status")]
    public class EventHACSStatus: Event
    {
        public Attributes data { get; set; }
    }
}