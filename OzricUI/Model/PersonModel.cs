using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class PersonModel: GraphNodeModel
{
    public const string ICON = "mdi:person";

    public PersonModel(Person sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}