using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class SkyBrightnessModel: GraphNodeModel
{
    public static string ICON = "ph:cloud-sun-bold";

    public SkyBrightnessModel(SkyBrightness skyBrightness, Point? point = null): base(skyBrightness, point)
    {
        
    }

    public override string Icon => ICON;
}