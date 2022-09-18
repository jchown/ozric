using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

internal class PinPortOutput: PinPort, IPort
{
    public bool input => false;
    public string cssClass => "output-port";

    public PinPortOutput(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is IPort { input: true } input && valueType == input.valueType;
    }
}