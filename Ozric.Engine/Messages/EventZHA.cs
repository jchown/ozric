namespace Ozric.Engine
{
    [TypeKey("zha_event")]
    public class EventZHA: Event
    {
        public EventZHAData data { get; set; }
    }
}