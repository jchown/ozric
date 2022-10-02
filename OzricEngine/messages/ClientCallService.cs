using System;
using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("call_service")]
    public class ClientCallService : ClientCommand, IMergable, IEquatable<ClientCallService>
    {
        public ClientCallService() : base("call_service") { }
        
        public string? domain { get; set; }
        public string? service { get; set; }
        public Attributes service_data { get; set; } = new();
        public Attributes target { get; set; } = new();

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

            var entityID = GetEntities();
            entityID.AddRange(other.target["entity_id"] as List<string> ?? throw new Exception("Missing entity_id"));
            entityID.Sort();
            return true;
        }

        public List<string> GetEntities()
        {
            return (List<string>) target["entity_id"] ?? throw new Exception("Missing entity_id");
        }

        public bool Equals(ClientCallService? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return domain == other.domain && service == other.service && Equals(service_data, other.service_data) && Equals(target, other.target);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ClientCallService)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(domain, service, service_data, target);
        }
    }
}