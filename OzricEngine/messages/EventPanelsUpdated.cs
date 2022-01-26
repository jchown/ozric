namespace OzricEngine
{
    [TypeKey("panels_updated")]
    public class EventPanelsUpdated: Event
    {
        public Attributes data { get; set; }
    }
}