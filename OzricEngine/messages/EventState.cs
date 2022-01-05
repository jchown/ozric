using System;
using System.Collections.Generic;

namespace OzricEngine
{
    public class EventState
    {
        public string entity_id { get; set; }
        
        public DateTime last_changed { get; set; }
        public DateTime last_updated { get; set; }
        
        public string model { get; set; }
        public string manufacturer { get; set; }
        
        public string state { get; set; }
        public Dictionary<string,object> attributes { get; set; }
    }
}