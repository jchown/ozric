using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public abstract class EntityModel: GraphNodeModel
{
    public EntityModel(EntityNode node, Point? point = null) : base(node, point)
    {
    }
}