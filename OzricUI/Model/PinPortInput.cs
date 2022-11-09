using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class PinPortInput: PinPort, IPort
{
    public string Name => name;
    public bool IsInput => true;
    public string CssClass => "input-port";

    public PinPortInput(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is IPort { IsInput: false } output && valueType == output.valueType;
    }
}