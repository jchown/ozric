namespace OzricEngine
{
    [TypeKey("persistent_notifications_updated")]
    public class EventPersistentNotificationsUpdated: Event
    {
        public Attributes data { get; set; }
    }
}