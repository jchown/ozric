using System.Text.Json.Serialization;

namespace Ozric.Engine.Values;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum ValueType
{
    Binary, Number, Color, Mode
}