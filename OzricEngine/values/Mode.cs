using System;
using System.Text.Json;

namespace OzricEngine.logic
{
    /// <summary>
    /// A single string tag to represent a state of operation, scene, etc.
    /// </summary>
    public sealed class Mode: Value, IEquatable<Mode>
    {
        public override ValueType ValueType => ValueType.Mode;

        public string value { get; }

        public Mode(string value)
        {
            this.value = value;
        }
                
        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            writer.WriteString("value", value);
        }

        public static Value ReadFromJSON(ref Utf8JsonReader reader)
        {
            if (!reader.Read() || reader.GetString() != "value" || !reader.Read())
                throw new Exception();
            
            return new Mode(reader.GetString());
        }
        
        public bool Equals(Mode? other)
        {
            return value == other.value;
        }

        public override bool Equals(object? obj)
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
        
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        
        public override string ToString()
        {
            return value;
        }
    }
}