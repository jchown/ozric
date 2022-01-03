using System.Collections.Generic;

namespace OzricEngine
{
    public class ClientCallService : ClientCommand
    {
        public ClientCallService() : base("call_service") { }
        
        public string domain { get; set; }
        public string service { get; set; }
        public Dictionary<string, object> service_data { get; set; }
        public Dictionary<string, string> target { get; set; }
    }
}