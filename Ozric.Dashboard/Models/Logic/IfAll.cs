using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Logic;

namespace Ozric.Dashboard.Model;

public class IfAll: VariableInputs
{
    public IfAll(Engine.Graph.Logic.IfAll ifAll, Point? point = null): base(ifAll, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:gate-and";
}