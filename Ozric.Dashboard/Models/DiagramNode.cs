using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using MudBlazor;
using Ozric.Dashboard.Components;
using Ozric.Dashboard.Shared;
using Ozric.Engine.Extensions;
using Ozric.Engine.Graph;
using LogLevel = Ozric.Engine.Utils.LogLevel;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(NodeDialog), "Node")]
public abstract class DiagramNode: NodeModel
{
    public readonly GraphNode graphNode;
    
    protected bool _inputLabels;
    protected bool _outputLabels;
    
    private readonly Mapping<Pin, PortModel> _portMappings;
    
    public bool HasAlert { get; private set; }

    public Color AlertColor { get; private set; }

    public DiagramNode(GraphNode graphNode, Point? point = null) : base(graphNode.id, point)
    {
        this.graphNode = graphNode;
        _portMappings = new();

        foreach (var input in graphNode.inputs)
            AddInput(input);
            
        foreach (var output in graphNode.outputs)
            AddOutput(output);

        _inputLabels = graphNode.inputs.Count > 1;
        _outputLabels = graphNode.outputs.Count > 1;

        Load();
        UpdateAlerts();
    }

    public abstract string Icon { get; }

    public virtual void Load()
    {
        Title = graphNode.id;
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

    public void UpdateAlerts()
    {
        HasAlert = graphNode.Alerts.Count > 0;
        if (!HasAlert)
        {
            AlertColor = Color.Transparent;
            return;
        }


        AlertColor = graphNode.Alerts.Select(a => a.Level).Max() switch
        {
            LogLevel.Trace => Color.Info,
            LogLevel.Debug => Color.Info,
            LogLevel.Info => Color.Info,

            LogLevel.Warning => Color.Warning,

            LogLevel.Error => Color.Error,
            LogLevel.Fatal => Color.Error,
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}