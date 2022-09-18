using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.ext;
using OzricEngine.Nodes;
using OzricUI.Shared;

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

    public void AddInput(Pin pin)
    {
        var port = AddPort(new InputPortModel(this, pin, PortAlignment.Left));
        portMappings.Add(pin, port);
    }

    private void AddOutput(Pin pin)
    {
        var port = AddPort(new OutputPortModel(this, pin, PortAlignment.Right));
        portMappings.Add(pin, port);
    }

    
    public abstract string Icon { get; }
}