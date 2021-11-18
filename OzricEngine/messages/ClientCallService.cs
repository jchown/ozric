using System.Collections.Generic;

namespace OzricEngine
{
    public class ClientCallService : ClientCommand
    {
        public string domain { get; set; } = null;
        public string service { get; set; } = null;
        public string event_type { get; set; } = null;
        public Dictionary<string, string> service_data { get; set; } = null;
        public Dictionary<string, string> target { get; set; } = null;

        public ClientCallService() : base("call_service")
        {
        }
    }
}