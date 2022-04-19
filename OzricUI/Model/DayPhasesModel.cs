using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class DayPhasesModel: GraphNodeModel
{
    public DayPhasesModel(DayPhases dayPhases, Point? point = null): base(dayPhases, point)
    {
        AddPort(new OutputMode(DayPhases.OUTPUT_NAME, this));
    }
}