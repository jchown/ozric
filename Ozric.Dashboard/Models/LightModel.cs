using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Entities;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(LightDialog), "Light")]
public class LightModel: EntityModel, IAreaSink
{
    
    public const string ICON = "mdi:lightbulb";

    public LightModel(Light light, Point? point = null): base(light, point)
    {
    }
    
    public override string Icon => ICON;
}