using Blazor.Diagrams.Core.Behaviors;

namespace Ozric.Dashboard.Diagram;

public class LinkDragBehavior: DragNewLinkBehavior
{
    public LinkDragBehavior(Blazor.Diagrams.Core.Diagram diagram) : base(diagram)
    {
    }
}