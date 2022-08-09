using Blazor.Diagrams.Core.Models;

namespace OzricUI.Model;

internal class InputBoolean : InputPortModel
{
    public InputBoolean(string name, NodeModel parent) : base($"{parent.Id}.{name}",parent, PortAlignment.Left)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is OutputBoolean;
    }
}