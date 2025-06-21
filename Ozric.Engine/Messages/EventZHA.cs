namespace OzricEngine
{
    [TypeKey("zha_event")]
    public class EventZHA: Event
    {
        public EventZHAData data { get; set; }
    }
}