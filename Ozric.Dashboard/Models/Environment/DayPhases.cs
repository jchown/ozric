using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(DayPhasesDialog), "Day Phases")]
public class DayPhases: DiagramNode
{
    public static string ICON = "mdi:weather-sunset";

    public DayPhases(OzricEngine.Nodes.DayPhases dayPhases, Point? point = null): base(dayPhases, point)
    {
    }

    public override string Icon => ICON;
}