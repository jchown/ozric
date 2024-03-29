using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    /// <summary>
    /// Base class for all "value" types that are used in the inputs & outputs of [OzricEngine.node.Node]s.
    /// These classes must be immutable & JSON convertible.
    /// </summary>
    public abstract class Value
    {
        [JsonPropertyOrder(-999)]
        public abstract ValueType ValueType { get; }

        public abstract void WriteAsJSON(Utf8JsonWriter writer);
        
        /// <summary>
        /// Force definition in subclasses
        /// </summary>

        public abstract override bool Equals(object? obj);

        /// <summary>
        /// Force definition in subclasses
        /// </summary>
        public abstract override int GetHashCode();

        public static bool operator ==(Value? lhs, Value? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Value? lhs, Value? rhs) => !(lhs == rhs);
    }
}