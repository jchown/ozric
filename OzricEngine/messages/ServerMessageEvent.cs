using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    [TypeKey("event")]
    public class ServerMessageEvent: ServerMessage
    {
        public int id { get; set; }
        public string origin { get; set; }
        public DateTime time_fired { get; set; }
        public EventDataContext DataContext { get; set; }

        [JsonPropertyName("event")]
        public Event payload { get; set; }
    }
}