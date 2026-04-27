namespace Ozric.Engine
{
    [TypeKey("state_changed")]
    public class EventStateChanged: Event
    {
        public EventData data { get; set; }
    }
}