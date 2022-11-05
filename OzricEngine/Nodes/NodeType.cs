using System.Text.Json.Serialization;

namespace OzricEngine.Nodes;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum NodeType
{
    BinaryChoice,
    BinarySustain,
    Constant, 
    DayPhases, 
    IfAll, 
    IfAny, 
    Light, 
    MediaPlayer,
    ModeSensor,
    ModeMatch,
    ModeSwitch, 
    Person,
    Sensor, 
    SkyBrightness,
    Switch,
    Tween
}