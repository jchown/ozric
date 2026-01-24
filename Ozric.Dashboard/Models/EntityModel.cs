using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Entities;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(EntityDialog), "Entity")]
public abstract class EntityModel: GraphNodeModel
{
    public string EntityID => ((EntityNode)node).entityID;
    
    public EntityModel(EntityNode node, Point? point = null) : base(node, point)
    {
    }
}