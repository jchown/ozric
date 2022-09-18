using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public interface IPort
{
    public ValueType valueType { get; }
    
    public string cssClass { get; }

    public bool input { get; }
    
    public bool hiddenIfLocked { get; }
}