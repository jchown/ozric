using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Entities;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(EntityDialog), "Entity")]
public abstract class DiagramEntity: DiagramNode
{
    public string EntityID => ((GraphEntity)graphNode).entityID;
    
    public DiagramEntity(GraphEntity graphNode, Point? point = null) : base(graphNode, point)
    {
    }
}