namespace OzricEngine
{
    public class EventData
    {
        public string event_id { get; set; }
        public EventState old_state { get; set; }
        public EventState new_state { get; set; }
    }
}