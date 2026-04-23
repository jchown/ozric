using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramSwitch: DiagramNode
{
    public const string ICON = "mdi:electric-switch";

    public DiagramSwitch(OzricEngine.Nodes.GraphSwitch graphSwitch, Point? point = null): base(graphSwitch, point)
    {
    }
    
    public override string Icon => ICON;
}