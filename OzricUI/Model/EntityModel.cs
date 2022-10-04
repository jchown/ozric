using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public abstract class EntityModel: GraphNodeModel
{
    public string EntityID => ((EntityNode)node).entityID;
    
    public EntityModel(EntityNode node, Point? point = null) : base(node, point)
    {
    }
}