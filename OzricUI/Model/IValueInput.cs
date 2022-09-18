using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public interface IValueInput
{
    public ValueType valueType { get;  }
}