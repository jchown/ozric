using System;
using System.Text.Json;

namespace OzricEngine.Values
{
    public sealed class Boolean: Value, IEquatable<Boolean>
    {
        public override ValueType ValueType => ValueType.Boolean;
        
        public bool value { get; }

        public Boolean(bool value)
        {
            this.value = value;
        }

        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            writer.WriteBoolean("value", value);
        }

        public static Value ReadFromJSON(ref Utf8JsonReader reader)
        {
            if (!reader.Read() || reader.GetString() != "value" || !reader.Read())
                throw new Exception();
            
            return new Boolean(reader.GetBoolean());
        }

        public override string ToString()
        {
            return value ? "True" : "False";
        }

        public static bool operator ==(Boolean? lhs, Boolean? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Boolean? lhs, Boolean? rhs) => !(lhs == rhs);
        
        public bool Equals(Boolean? other)
        {
            return (other != null) && (value == other.value);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Boolean other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}