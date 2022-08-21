using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ColorMode
    {
        HS, Temp, RGB, XY
    }
}