using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ColorType
    {
        HS, Temp, RGB, XY
    }
}