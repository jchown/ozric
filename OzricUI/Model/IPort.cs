using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public interface IPort
{
    public ValueType valueType { get; }
    
    public string Name { get; }
    
    public string CssClass { get; }

    public bool IsInput { get; }
    
    public bool HiddenIfLocked { get; }
}