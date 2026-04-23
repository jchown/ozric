using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramSwitch: DiagramNode
{
    public const string ICON = "mdi:electric-switch";

    public DiagramSwitch(OzricEngine.Nodes.GraphSwitch @switch, Point? point = null): base(@switch, point)
    {
    }
    
    public override string Icon => ICON;
}