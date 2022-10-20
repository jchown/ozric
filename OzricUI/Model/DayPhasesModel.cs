using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(DayPhasesDialog), "Day Phases")]
public class DayPhasesModel: GraphNodeModel
{
    public static string ICON = "mdi:weather-sunset";

    public DayPhasesModel(DayPhases dayPhases, Point? point = null): base(dayPhases, point)
    {
    }

    public override string Icon => ICON;
}