using Blazor.Diagrams.Core.Models;

namespace OzricUI.Model;

internal class OutputMode : OutputPortModel
{
    public OutputMode(string name, NodeModel parent) : base($"{parent.Id}.{name}",parent, PortAlignment.Right)
    {
    }

    public override bool CanAttachTo(PortModel port)
    {
        return port is InputMode;
    }
}