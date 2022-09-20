using System.Text.Json.Serialization;

namespace OzricEngine.Nodes;

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
    BooleanChoice,
    Switch,
    MediaPlayer,
    ModeSensor,
    ModeMatch
}