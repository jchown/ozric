using System;

namespace OzricEngine.logic
{
    /// <summary>
    /// A single string tag to represent a state of operation, scene, etc.
    /// </summary>
    public sealed class Mode: Value, IEquatable<Mode>
    {
        public override ValueType ValueType => ValueType.Mode;

        public string value { get; }

        public Mode()
        {
        }
        
        public Mode(string value)
        {
            this.value = value;
        }
        
        public bool Equals(Mode other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Mode other && Equals(other);
        }

        public static bool operator ==(Mode lhs, Mode rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Mode lhs, Mode rhs) => !(lhs == rhs);
        
        public override string ToString()
        {
            return value;
        }
    }
}