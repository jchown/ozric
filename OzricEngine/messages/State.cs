using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.ext;

namespace OzricEngine
{
    public class State
    {
        public string entity_id { get; set; }
        public string state { get; set; }
        public Dictionary<string, object> attributes { get; set; }
        public DateTime last_changed { get; set; }
        public DateTime last_updated { get; set; }
        public StateContext context { get; set; }

        [JsonIgnore]
        public string Name => (attributes.Get("friendly_name")?.ToString() ?? entity_id).Trim();
        
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}