using Blazor.Diagrams;
using Blazor.Diagrams.Core.Behaviors;

namespace Ozric.Dashboard.Diagram;

public static class DiagramExt
{
    public static void AddConstraintBehaviour(this BlazorDiagram diagram)
    {
        var existing = diagram.GetBehavior<DragMovablesBehavior>();
        if (existing != null)
        {
            diagram.UnregisterBehavior<DragMovablesBehavior>();
        }
        
        diagram.RegisterBehavior(new ConstraintBehavior(diagram));
    }
}