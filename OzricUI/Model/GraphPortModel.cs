using Blazor.Diagrams.Core.Models;
using OzricEngine.logic;

namespace OzricUI.Model;

public abstract class GraphPortModel: PortModel
{
    public readonly ValueType valueType;
    public readonly string name;

    public GraphPortModel(NodeModel parent, Pin pin, PortAlignment alignment) : base($"{parent.Id}.{pin.id}", parent, alignment)
    {
        name = pin.name;
        valueType = pin.type;
    }
}