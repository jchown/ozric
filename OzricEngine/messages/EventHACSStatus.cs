using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("hacs/status")]
    public class EventHACSStatus: Event
    {
        public Attributes data { get; set; }
    }
}