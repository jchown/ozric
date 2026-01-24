using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Ozric.Engine.Graph;

namespace Ozric.Dashboard.Model;

public class PinPortInput: PinPort, IPort
{
    public string Name => name;
    public bool IsInput => true;
    public string CssClass => "input-port";

    public PinPortInput(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(ILinkable port)
    {
        return port is IPort { IsInput: false } output && valueType == output.valueType;
    }
}