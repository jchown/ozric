using Blazor.Diagrams.Core.Geometry;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Model;

public class DiagramBinaryChoice: DiagramNode
{
    public DiagramBinaryChoice(Engine.Graph.Logic.GraphBinaryChoice choice, Point? point = null): base(choice, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:call-split";

    public static Engine.Graph.Logic.GraphBinaryChoice Color(string nodeID)
    {
        return new Engine.Graph.Logic.GraphBinaryChoice(nodeID, ValueType.Color);
    }
}