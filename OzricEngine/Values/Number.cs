using System;
using System.Text.Json;

namespace OzricEngine.Values
{
    public sealed class Number: Value, IEquatable<Number>
    {
        public override ValueType ValueType => ValueType.Number;

        public float value { get; }

        public Number(float value)
        {
            this.value = value;
        }
        
        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            writer.WriteNumber("value", value);
        }

        public static Value ReadFromJSON(ref Utf8JsonReader reader)
        {
            if (!reader.Read() || reader.GetString() != "value" || !reader.Read())
                throw new Exception();
            
            return new Number(reader.GetSingle());
        }
        
        public static Value ReadFromJSON(JsonDocument document)
        {
            var value = document.RootElement.GetProperty("value");
            return new Number(value.GetSingle());
        }
        
        public static bool operator ==(Number? lhs, Number? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Number? lhs, Number? rhs) => !(lhs == rhs);

        public bool Equals(Number? other)
        {
            return (other != null) && (value == other.value);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Number other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}