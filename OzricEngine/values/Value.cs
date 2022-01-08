using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// Base class for all "value" types that are used in the inputs & outputs of [OzricEngine.node.Node]s.
    /// These classes must be immutable & JSON convertable.
    /// </summary>
    public abstract class Value
    {
        [JsonIgnore]
        public abstract ValueType ValueType { get; }
    }
}