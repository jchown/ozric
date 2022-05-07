using Blazor.Diagrams.Core.Models;

namespace OzricUI.Model;

public class InputPortModel: PortModel
{
    protected InputPortModel(string id, NodeModel parent, PortAlignment alignment) : base(id, parent, alignment)
    {
    }
}