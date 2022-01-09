using System;

namespace OzricEngine
{
    public class InputSelector: IEquatable<InputSelector>
    {
        public string nodeID { get; set; }
        public string inputName { get; set; }

        public static bool operator ==(InputSelector lhs, InputSelector rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(InputSelector lhs, InputSelector rhs) => !(lhs == rhs);

        public bool Equals(InputSelector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return nodeID == other.nodeID && inputName == other.inputName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InputSelector)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nodeID, inputName);
        }
    }
}