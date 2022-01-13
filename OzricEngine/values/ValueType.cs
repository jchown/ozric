using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ValueType
    {
        Scalar, OnOff, Color, Mode
    }
}