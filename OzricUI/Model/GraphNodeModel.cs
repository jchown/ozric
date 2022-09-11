using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.ext;
using OzricEngine.logic;
using OzricUI.Shared;
using ValueType = OzricEngine.logic.ValueType;

namespace OzricUI.Model;

public abstract class GraphNodeModel: NodeModel
{
    private Node node;
    private Mapping<Pin, PortModel> portMappings;

    public GraphNodeModel(Node node, Point? point = null) : base(node.id, point)
    {
        this.node = node;
        portMappings = new();

        foreach (var input in node.inputs)
            AddInput(input);
            
        foreach (var output in node.outputs)
            AddOutput(output);

        Load();
    }

    public virtual void Load()
    {
        Title = node.id;
    }

    public PortModel GetPort(string id)
    {
        return Ports.FirstOrDefault(p => p.Id == id) ?? throw new Exception($"Port id {id} not found in [{Ports.Select(p => p.Id).Join(",")}]");
    }

    public Pin GetPin(PortModel port)
    {
        return portMappings.GetGraph(port);
    }

    private void AddInput(Pin pin)
    {
        var port = CreateInputPort(pin.type, pin.name);
        portMappings.Add(pin, port);
    }

    private void AddOutput(Pin pin)
    {
        var port = CreateOutputPort(pin.type, pin.name);
        portMappings.Add(pin, port);
    }

    private PortModel CreateInputPort(ValueType valueType, string name)
    {
        switch (valueType)
        {
            case ValueType.Color:
                return AddPort(new InputColor(name, this));

            case ValueType.Boolean:
                return AddPort(new InputBoolean(name, this));

            case ValueType.Mode:
                return AddPort(new InputMode(name, this));

            default:
                throw new Exception($"Unknown type: {valueType}");
        }
    }

    private PortModel CreateOutputPort(ValueType valueType, string name)
    {
        switch (valueType)
        {
            case ValueType.Color:
                return AddPort(new OutputColor(name, this));

            case ValueType.Boolean:
                return AddPort(new OutputBoolean(name, this));

            case ValueType.Mode:
                return AddPort(new OutputMode(name, this));

            default:
                throw new Exception($"Unknown type: {valueType}");
        }
    }
    
    public abstract string Icon { get; }
}