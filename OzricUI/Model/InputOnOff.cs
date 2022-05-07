using Blazor.Diagrams.Core.Models;

namespace OzricUI.Model;

internal class InputOnOff : InputPortModel
{
    public InputOnOff(string name, NodeModel parent) : base($"{parent.Id}.{name}",parent, PortAlignment.Left)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is OutputOnOff;
    }
}