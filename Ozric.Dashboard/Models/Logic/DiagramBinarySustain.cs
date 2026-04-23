using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(BinarySustainDialog), "Binary Sustain")]
public class DiagramBinarySustain: DiagramNode
{
    public DiagramBinarySustain(Engine.Graph.Logic.GraphBinarySustain bs, Point? point = null): base(bs, point)
    {
    }
        
    public override string Icon => ICON;
    public const string ICON = "ic:baseline-linear-scale";
}