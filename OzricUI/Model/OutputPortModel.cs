using Blazor.Diagrams.Core.Models;

namespace OzricUI.Model;

internal class OutputPortModel: PortModel
{
    protected OutputPortModel(string id, NodeModel parent, PortAlignment alignment) : base(id, parent, alignment)
    {
    }
}