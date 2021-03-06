using Blazor.Diagrams.Core.Models;

namespace OzricUI.Model;

internal class InputMode : InputPortModel
{
    public InputMode(string name, NodeModel parent) : base($"{parent.Id}.{name}",parent, PortAlignment.Left)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is OutputMode;
    }
}