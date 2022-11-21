using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class WeatherModel: EntityModel
{
    public static string ICON = "mdi:weather-partly-snowy-rainy";

    public WeatherModel(Weather weather, Point? point = null): base(weather, point)
    {
    }

    public override string Icon => ICON;
}