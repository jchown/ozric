using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(ModeMatchDialog), "Mode Match")]
public class ModeMatchModel: GraphNodeModel
{
    public ModeMatchModel(ModeMatch modeMatch, Point? point = null): base(modeMatch, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:checklist";
}