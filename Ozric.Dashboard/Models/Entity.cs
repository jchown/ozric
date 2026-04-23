using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Entities;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(EntityDialog), "Entity")]
public abstract class Entity: DiagramNode
{
    public string EntityID => ((EntityGraphNode)graphNode).entityID;
    
    public Entity(EntityGraphNode graphNode, Point? point = null) : base(graphNode, point)
    {
    }
}