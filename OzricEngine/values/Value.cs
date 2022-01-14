using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// Base class for all "value" types that are used in the inputs & outputs of [OzricEngine.node.Node]s.
    /// These classes must be immutable & JSON convertable.
    /// </summary>
    public abstract class Value
    {
        [JsonPropertyName("value-type")]
        public abstract ValueType ValueType { get; }

        public abstract void WriteAsJSON(Utf8JsonWriter writer);
        
        public static bool operator ==(Value lhs, Value rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Value lhs, Value rhs) => !(lhs == rhs);

    }
}