using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class DayPhasesModel: GraphNodeModel
{
    public DayPhasesModel(DayPhases dayPhases, Point? point = null): base(dayPhases, point)
    {
    }

    public override string Icon => "mdi:weather-sunset";
}