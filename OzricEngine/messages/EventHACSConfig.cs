namespace OzricEngine
{
    [TypeKey("hacs/config")]
    public class EventHACSConfig: Event
    {
        public Attributes data { get; set; }
    }
}