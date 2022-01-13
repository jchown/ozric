namespace OzricEngine
{
    [TypeKey("hacs/repository")]
    public class EventHACSRepository: Event
    {
        public Attributes data { get; set; }
    }
}