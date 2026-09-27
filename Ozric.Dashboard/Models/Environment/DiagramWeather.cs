using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Environment;

namespace Ozric.Dashboard.Model;

public class DiagramWeather: DiagramEntity
{
    public static string ICON = "mdi:weather-partly-snowy-rainy";

    public DiagramWeather(GraphWeather weather, Point? point = null): base(weather, point)
    {
    }

    public override string Icon => ICON;
}