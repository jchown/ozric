using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class Weather: Entity
{
    public static string ICON = "mdi:weather-partly-snowy-rainy";

    public Weather(OzricEngine.Nodes.Weather weather, Point? point = null): base(weather, point)
    {
    }

    public override string Icon => ICON;
}