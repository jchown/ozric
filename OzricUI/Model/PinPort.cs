using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public abstract class PinPort: PortModel
{
    public ValueType valueType { get; }
    public readonly string name;
    public bool hiddenIfLocked => false;

    public PinPort(NodeModel parent, Pin pin, PortAlignment alignment) : base($"{parent.Id}.{pin.id}", parent, alignment)
    {
        name = pin.name;
        valueType = pin.type;
    }
}
