using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("panels_updated")]
    public class EventPanelsUpdated: Event
    {
        public Attributes data { get; set; }
    }
}