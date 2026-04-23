using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class SkyBrightness: DiagramNode
{
    public static string ICON = "ph:cloud-sun-bold";

    public SkyBrightness(OzricEngine.Nodes.SkyBrightness skyBrightness, Point? point = null): base(skyBrightness, point)
    {
        _outputLabels = true;
    }

    public override string Icon => ICON;
}