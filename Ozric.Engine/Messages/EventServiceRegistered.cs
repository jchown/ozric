using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("service_registered")]
    public class EventServiceRegistered: Event
    {
        public Attributes data { get; set; }
    }
}