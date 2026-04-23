using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ModeSwitchDialog), "Mode Switch")]
public class DiagramModeSwitch: DiagramNode
{
    public static string ICON = "fluent:table-switch-28-regular";

    public DiagramModeSwitch(Engine.Graph.Logic.GraphModeSwitch s, Point? point = null): base(s, point)
    {
    }
    
    public override string Icon => ICON;
    
    public static Engine.Graph.Logic.GraphModeSwitch Color(string nodeID)
    {
        var node = new Engine.Graph.Logic.GraphModeSwitch(nodeID);
        node.AddOutput("colour-on", ValueType.Color);
        node.AddOutput("colour-off", ValueType.Color);
        return node;
    }
}