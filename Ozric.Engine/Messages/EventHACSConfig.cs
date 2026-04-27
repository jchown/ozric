using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("hacs/config")]
    public class EventHACSConfig: Event
    {
        public Attributes data { get; set; }
    }
}