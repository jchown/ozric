using System;
using System.Collections.Generic;
using System.Text.Json;

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
        public Attributes attributes { get; set; }
        
        public override string ToString()
        {
            return Json.Serialize(this);
        }

        public bool IsRedacted()
        {
            return entity_id.EndsWith("_geocoded_location");
        }
    }
}