namespace OzricEngine
{
    [TypeKey("component_loaded")]
    public class EventComponentLoaded: Event
    {
        public Attributes data { get; set; }
    }
}