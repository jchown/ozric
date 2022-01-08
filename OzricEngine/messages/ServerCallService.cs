using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    [TypeKey("call_service")]
    public class ServerCallService : ServerMessage
    {
        public ServerCallService() { }
        
        public string domain { get; set; }
        public string service { get; set; }
        
        [JsonConverter(typeof(JsonConverterDictionaryObjectValues))]
        public Dictionary<string, object> service_data { get; set; }
        public Dictionary<string, string> target { get; set; }
    }
}