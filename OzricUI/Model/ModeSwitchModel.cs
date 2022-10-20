using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using OzricUI.Components;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

[EditDialog(typeof(ModeSwitchDialog), "Mode Switch")]
public class ModeSwitchModel: GraphNodeModel
{
    public static string ICON = "fluent:table-switch-28-regular";

    public ModeSwitchModel(ModeSwitch s, Point? point = null): base(s, point)
    {
    }
    
    public override string Icon => ICON;
    
    public static ModeSwitch Color(string nodeID)
    {
        var node = new ModeSwitch(nodeID);
        node.AddOutput("colour-on", ValueType.Color);
        node.AddOutput("colour-off", ValueType.Color);
        return node;
    }
}