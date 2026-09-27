using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Environment;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(DayPhasesDialog), "Day Phases")]
public class DiagramDayPhases: DiagramNode
{
    public static string ICON = "mdi:weather-sunset";

    public DiagramDayPhases(GraphDayPhases dayPhases, Point? point = null): base(dayPhases, point)
    {
    }

    public override string Icon => ICON;
}