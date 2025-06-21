using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

internal class PinPortOutput: PinPort, IPort
{
    public string Name => name;
    public bool IsInput => false;
    public string CssClass => "output-port";

    public PinPortOutput(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(ILinkable port)
    {
        return port is IPort { IsInput: true } input && valueType == input.valueType;
    }
}