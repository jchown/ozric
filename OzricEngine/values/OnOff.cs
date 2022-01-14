using System;
using System.Text.Json;

namespace OzricEngine.logic
{
    public sealed class OnOff: Value, IEquatable<OnOff>
    {
        public override ValueType ValueType => ValueType.OnOff;

        public bool value { get; }

        public OnOff()
        {
        }

        public OnOff(bool value)
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
            
            return new OnOff(reader.GetBoolean());
        }

        public override string ToString()
        {
            return value ? "On" : "Off";
        }

        public static bool operator ==(OnOff lhs, OnOff rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(OnOff lhs, OnOff rhs) => !(lhs == rhs);
        
        public bool Equals(OnOff other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OnOff other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}