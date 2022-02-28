using System.Text.Json.Serialization;

namespace OzricEngine
{
    [TypeKey("event")]
    public class ServerEvent: ServerMessage
    {
        public int id { get; set; }

        [JsonPropertyName("event")]
        public Event payload { get; set; }
    }
}