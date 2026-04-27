using System.Text.Json.Serialization;

namespace Ozric.Engine.Values
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum ColorMode
    {
        HS, Temp, RGB, XY,
        Unknown
    }
}