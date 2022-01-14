using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum NodeType
    {
        Constant, 
        DayPhases, 
        IfAll, 
        IfAny, 
        Light, 
        ModeSwitch, 
        Sensor, 
        SkyBrightness,
        Switch,
    }
}