using System.Text.Json.Serialization;

namespace Ozric.Engine
{
    [TypeKey("event")]
    public class ServerEvent: ServerMessage
    {
        public int id { get; set; }

        [JsonPropertyName("event")]
        public Event payload { get; set; }
    }
}