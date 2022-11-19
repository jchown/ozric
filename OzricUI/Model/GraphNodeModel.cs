using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.ext;
using OzricEngine.Nodes;
using OzricUI.Components;
using OzricUI.Shared;

namespace OzricUI.Model;

[EditDialog(typeof(NodeDialog), "Node")]
public abstract class GraphNodeModel: NodeModel
{
    public readonly Node node;
    protected bool _inputLabels;
    protected bool _outputLabels;
    
    private readonly Mapping<Pin, PortModel> _portMappings;
    
    private static List<Type> nodeModelTypes = typeof(GraphNodeModel).Assembly.ExportedTypes
        .Where(t => typeof(GraphNodeModel).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
        .ToList();

    public GraphNodeModel(Node node, Point? point = null) : base(node.id, point)
    {
        this.node = node;
        _portMappings = new();

        foreach (var input in node.inputs)
            AddInput(input);
            
        foreach (var output in node.outputs)
            AddOutput(output);

        _inputLabels = node.inputs.Count > 1;
        _outputLabels = node.outputs.Count > 1;

        Load();
    }

    public abstract string Icon { get; }

    public virtual void Load()
    {
        Title = node.id;
    }

    public PortModel GetPort(string id)
    {
        return Ports.FirstOrDefault(p => p.Id == id) ?? throw new Exception($"Port id {id} not found in [{Ports.Select(p => p.Id).Join(",")}]");
    }
    
    public bool HasPin(PortModel port)
    {
        return _portMappings.HasDiagramModel(port);
    }

    public Pin GetPin(PortModel port)
    {
        return _portMappings.GetGraph(port);
    }

    public virtual void AddInput(Pin pin)
    {
        var port = AddPort(new PinPortInput(this, pin, PortAlignment.Left));
        _portMappings.Add(pin, port);
    }

    private void AddOutput(Pin pin)
    {
        var port = AddPort(new PinPortOutput(this, pin, PortAlignment.Right));
        _portMappings.Add(pin, port);
    }

    public void RemoveInput(Pin pin)
    {
        var port = _portMappings.GetDiagram(pin);
        RemovePort(port);
        _portMappings.Remove(pin);
    }
    
    public virtual int PortHeight()
    {
        int nl = 0, nr = 0;
        
        foreach (var port in Ports)
        {
            var ip = (IPort)port; 
            if (ip.HiddenIfLocked && Locked)
                continue;
            
            if (ip.IsInput)
                nl++;
            else
                nr++;
        }

        return Math.Max(nl, nr);
    }
    
    public virtual int GetPortPosition(IPort port)
    {
        var input = port.IsInput;
        int position = 0;
        foreach (var p in Ports)
        {
            if (p == port)
                return position;

            if (((IPort)p).IsInput == input)
                position++;
        }

        throw new Exception();
    }

    public bool ShowLabel(IPort port)
    {
        return port.IsInput ? _inputLabels : _outputLabels;
    }

    public static List<Type> GetDerivatives()
    {
        return nodeModelTypes;
    }
}