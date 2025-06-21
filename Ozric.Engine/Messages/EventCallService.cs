using OzricEngine.Nodes;

namespace OzricEngine
{
    [TypeKey("call_service")]
    public class EventCallService: Event
    {
        public EventCallServiceData data { get; set; }
    }
}