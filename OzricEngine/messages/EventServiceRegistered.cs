namespace OzricEngine
{
    [TypeKey("service_registered")]
    public class EventServiceRegistered: Event
    {
        public Attributes data { get; set; }
    }
}