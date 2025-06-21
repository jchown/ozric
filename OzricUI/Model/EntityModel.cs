using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Nodes.Entities;
using OzricEngine.Nodes;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(EntityDialog), "Entity")]
public abstract class EntityModel: GraphNodeModel
{
    public string EntityID => ((EntityNode)node).entityID;
    
    public EntityModel(EntityNode node, Point? point = null) : base(node, point)
    {
    }
}