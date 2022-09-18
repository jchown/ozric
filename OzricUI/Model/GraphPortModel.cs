using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public abstract class GraphPortModel: PortModel
{
    public ValueType valueType { get; }
    
    public readonly string name;

    public GraphPortModel(NodeModel parent, Pin pin, PortAlignment alignment) : base($"{parent.Id}.{pin.id}", parent, alignment)
    {
        name = pin.name;
        valueType = pin.type;
    }
}