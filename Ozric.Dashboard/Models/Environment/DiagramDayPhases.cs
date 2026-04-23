using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(DayPhasesDialog), "Day Phases")]
public class DiagramDayPhases: DiagramNode
{
    public static string ICON = "mdi:weather-sunset";

    public DiagramDayPhases(OzricEngine.Nodes.GraphDayPhases dayPhases, Point? point = null): base(dayPhases, point)
    {
    }

    public override string Icon => ICON;
}