using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class ModeSwitchModel: GraphNodeModel
{
    public ModeSwitchModel(ModeSwitch s, Point? point = null): base(s, point)
    {
    }
    
    public override string Icon => "fluent:table-switch-28-regular";
}