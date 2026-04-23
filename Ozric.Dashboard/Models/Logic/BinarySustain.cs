using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Logic;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(BinarySustainDialog), "Binary Sustain")]
public class BinarySustain: DiagramNode
{
    public BinarySustain(Engine.Graph.Logic.BinarySustain bs, Point? point = null): base(bs, point)
    {
    }
        
    public override string Icon => ICON;
    public const string ICON = "ic:baseline-linear-scale";
}