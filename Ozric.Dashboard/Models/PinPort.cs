using Blazor.Diagrams.Core.Models;
using Ozric.Engine.Graph;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Model;

public abstract class PinPort: PortModel
{
    public ValueType valueType { get; }
    public readonly string name;
    public bool HiddenIfLocked => false;

    public PinPort(NodeModel parent, Pin pin, PortAlignment alignment) : base($"{parent.Id}.{pin.id}", parent, alignment)
    {
        name = pin.name;
        valueType = pin.type;
    }
}
