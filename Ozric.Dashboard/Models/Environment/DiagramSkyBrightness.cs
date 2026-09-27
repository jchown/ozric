using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Environment;

namespace Ozric.Dashboard.Model;

public class DiagramSkyBrightness: DiagramNode
{
    public static string ICON = "ph:cloud-sun-bold";

    public DiagramSkyBrightness(GraphSkyBrightness skyBrightness, Point? point = null): base(skyBrightness, point)
    {
        _outputLabels = true;
    }

    public override string Icon => ICON;
}