using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(BinarySustainDialog), "Binary Sustain")]
public class BinarySustainModel: GraphNodeModel
{
    public BinarySustainModel(BinarySustain bs, Point? point = null): base(bs, point)
    {
    }
        
    public override string Icon => ICON;

    public const string ICON = "mdi:linear-scale";
}