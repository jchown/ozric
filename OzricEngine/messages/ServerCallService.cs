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
        
        public Attributes service_data { get; set; }
        public Attributes target { get; set; }
    }
}