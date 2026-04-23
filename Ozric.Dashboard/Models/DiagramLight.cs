using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(LightDialog), "Light")]
public class DiagramLight: DiagramEntity, IAreaSink
{
    
    public const string ICON = "mdi:lightbulb";

    public DiagramLight(Engine.Graph.Entities.GraphLight graphLight, Point? point = null): base(graphLight, point)
    {
    }
    
    public override string Icon => ICON;
}