using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ValueType
    {
        Scalar, Boolean, Color, Mode
    }
}