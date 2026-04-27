using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("persistent_notifications_updated")]
    public class EventPersistentNotificationsUpdated: Event
    {
        public Attributes data { get; set; }
    }
}