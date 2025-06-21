using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(LightDialog), "Light")]
public class LightModel: EntityModel
{
    public const string ICON = "mdi:lightbulb";

    public LightModel(Light light, Point? point = null): base(light, point)
    {
    }
    
    public override string Icon => ICON;
}