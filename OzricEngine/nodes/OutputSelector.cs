using System;

namespace OzricEngine
{
    public class OutputSelector: IEquatable<OutputSelector>
    {
        public string nodeID { get; set; }
        public string outputName { get; set; }

        public static bool operator ==(OutputSelector lhs, OutputSelector rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(OutputSelector lhs, OutputSelector rhs) => !(lhs == rhs);

        public bool Equals(OutputSelector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return nodeID == other.nodeID && outputName == other.outputName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OutputSelector)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nodeID, outputName);
        }
    }
}