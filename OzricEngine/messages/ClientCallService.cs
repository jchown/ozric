using System;
using System.Collections.Generic;

namespace OzricEngine
{
    public class ClientCallService : ClientCommand, IMergable
    {
        public ClientCallService() : base("call_service") { }
        
        public string domain { get; set; }
        public string service { get; set; }
        public Attributes service_data { get; set; }
        public Attributes target { get; set; }
        
        public bool Merge(object o)
        {
            var other = o as ClientCallService;
            if (other == null)
                return false;

            if (domain != other.domain)
                return false;

            if (service != other.service)
                return false;

            if (service_data != other.service_data)
                return false;

            if (target.Count != 1 || !target.ContainsKey("entity_id"))
                return false;

            var entityID = target["entity_id"] as List<string> ?? throw new Exception("Missing entity_id");
            entityID.AddRange(other.target["entity_id"] as List<string> ?? throw new Exception("Missing entity_id"));
            return true;
        }
    }
}