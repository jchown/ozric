using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ColorMode
    {
        HS, Temp, RGB, XY
    }
}