using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("hacs/status")]
    public class EventHACSStatus: Event
    {
        public Dictionary<string, object> data { get; set; }
    }
}