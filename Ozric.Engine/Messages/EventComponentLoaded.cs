using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("component_loaded")]
    public class EventComponentLoaded: Event
    {
        public Attributes data { get; set; }
    }
}