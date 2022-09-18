using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class PinPortInput: PinPort, IPort
{
    public bool input => true;
    public string cssClass => "input-port";

    public PinPortInput(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is IPort { input: false } output && valueType == output.valueType;
    }
}