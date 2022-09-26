namespace OzricEngine.Nodes;

/// <summary>
/// If you add an entity type here, also add it to EntityState.GetCategory
/// </summary>
public enum Category
{
    Unknown,
    Light,
    Switch,
    Sensor,
    Logic,
    Constant,
    MediaPlayer,
    ModeSensor,
    Person
}