using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class Person: DiagramNode
{
    public const string ICON = "mdi:person";

    public Person(OzricEngine.Nodes.Person sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}