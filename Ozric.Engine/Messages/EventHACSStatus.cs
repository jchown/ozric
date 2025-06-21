using Ozric.Engine.Messages;

namespace OzricEngine
{
    [TypeKey("hacs/status")]
    public class EventHACSStatus: Event
    {
        public Attributes data { get; set; }
    }
}