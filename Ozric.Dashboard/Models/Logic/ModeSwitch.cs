using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Logic;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ModeSwitchDialog), "Mode Switch")]
public class ModeSwitch: DiagramNode
{
    public static string ICON = "fluent:table-switch-28-regular";

    public ModeSwitch(Engine.Graph.Logic.ModeSwitch s, Point? point = null): base(s, point)
    {
    }
    
    public override string Icon => ICON;
    
    public static Engine.Graph.Logic.ModeSwitch Color(string nodeID)
    {
        var node = new Engine.Graph.Logic.ModeSwitch(nodeID);
        node.AddOutput("colour-on", ValueType.Color);
        node.AddOutput("colour-off", ValueType.Color);
        return node;
    }
}