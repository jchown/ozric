using System;
using System.Runtime.Remoting.Contexts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    [TypeKey("event")]
    public class ServerEvent: ServerMessage
    {
        public int id { get; set; }
        public string origin { get; set; }
        public DateTime time_fired { get; set; }
        public StateContext data { get; set; }

        [JsonPropertyName("event")]
        public Event payload { get; set; }
    }
}