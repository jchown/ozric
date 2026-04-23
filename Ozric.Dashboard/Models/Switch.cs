using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class Switch: DiagramNode
{
    public const string ICON = "mdi:electric-switch";

    public Switch(OzricEngine.Nodes.Switch @switch, Point? point = null): base(@switch, point)
    {
    }
    
    public override string Icon => ICON;
}