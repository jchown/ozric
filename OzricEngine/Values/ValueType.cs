using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ValueType
    {
        Binary, Number, Color, Mode
    }
}