using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramWeather: DiagramEntity
{
    public static string ICON = "mdi:weather-partly-snowy-rainy";

    public DiagramWeather(Ozric.Engine.Nodes.GraphWeather weather, Point? point = null): base(weather, point)
    {
    }

    public override string Icon => ICON;
}