using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.logic;
using ValueType = OzricEngine.logic.ValueType;

namespace OzricUI.Model;

public class GraphNodeModel: NodeModel
{
    public GraphNodeModel(Node node, Point? point = null) : base(point)
    {
        Title = node.Name;
    }
    
    public void AddInput(ValueType valueType, string name)
    {
        switch (valueType)
        {
            case ValueType.Color:
            {
                AddPort(new InputColor(name, this));
                break;
            }
            case ValueType.OnOff:
            {
                AddPort(new InputOnOff(name, this));
                break;
            }
            default:
            {
                throw new Exception($"Unknown type: {valueType}");
            }
        }
    }

    public void AddOutput(ValueType valueType, string name)
    {
        switch (valueType)
        {
            case ValueType.Color:
            {
                AddPort(new OutputColor(name, this));
                break;
            }
            case ValueType.OnOff:
            {
                AddPort(new OutputOnOff(name, this));
                break;
            }
            default:
            {
                throw new Exception($"Unknown type: {valueType}");
            }
        }
    }

}