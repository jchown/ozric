using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class ModeMatchModel: GraphNodeModel
{
    public ModeMatchModel(ModeMatch modeMatch, Point? point = null): base(modeMatch, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:checklist";
}