namespace OzricEngine
{
    [TypeKey("hacs/stage")]
    public class EventHACSStage: Event
    {
        public Attributes data { get; set; }
    }
}