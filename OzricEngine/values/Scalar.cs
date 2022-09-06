using System;
using System.Text.Json;

namespace OzricEngine.logic
{
    public sealed class Scalar: Value, IEquatable<Scalar>
    {
        public override ValueType ValueType => ValueType.Scalar;

        public float value { get; }

        public Scalar()
        {
        }
        
        public Scalar(float value)
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
            
            return new Scalar(reader.GetSingle());
        }

        public static bool operator ==(Scalar? lhs, Scalar? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Scalar? lhs, Scalar? rhs) => !(lhs == rhs);

        public bool Equals(Scalar? other)
        {
            return (other != null) && (value == other.value);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Scalar other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}