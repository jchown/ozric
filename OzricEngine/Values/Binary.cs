using System;
using System.Text.Json;

namespace OzricEngine.Values
{
    public sealed class Binary: Value, IEquatable<Binary>
    {
        public override ValueType ValueType => ValueType.Binary;
        
        public bool value { get; }

        public Binary(bool value)
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
            
            return new Binary(reader.GetBoolean());
        }

        public static Value ReadFromJSON(JsonDocument document)
        {
            var value = document.RootElement.GetProperty("value");
            return new Binary(value.GetBoolean());
        }

        public override string ToString()
        {
            return value ? "True" : "False";
        }

        public static bool operator ==(Binary? lhs, Binary? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Binary? lhs, Binary? rhs) => !(lhs == rhs);
        
        public bool Equals(Binary? other)
        {
            return (other != null) && (value == other.value);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Binary other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}