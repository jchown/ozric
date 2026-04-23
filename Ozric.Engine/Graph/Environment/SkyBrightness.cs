using System;
using System.Threading.Tasks;
using Ozric.Engine.Graph;
using Ozric.Engine.Model;
using Ozric.Engine.Utils;
using OzricEngine.Values;
using ValueType = Ozric.Engine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// Combines sun elevation with cloud coverage to determine the overall sky brightness.
/// 1 = bright sunshine, 0 = darkness.
/// Uses a smoothstep curve on sun elevation and a multiplicative cloud occlusion model.
/// </summary>
[TypeKey(NodeType.SkyBrightness)]
public class SkyBrightness: GraphNode
{
    public override NodeType nodeType => NodeType.SkyBrightness;

    public const string Sun = "sun";
    public const string Clouds = "clouds";
    public const string Brightness = "brightness";

    public SkyBrightness(string id = "sky-brightness") : base(id, null, new() { new(Sun, ValueType.Number), new(Clouds, ValueType.Number), new(Brightness, ValueType.Number)})
    {
    }

    public override Task OnInit(Context context)
    {
        CalculateValue(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        CalculateValue(context);
        return Task.CompletedTask;
    }

    private void CalculateValue(Context context)
    {
        var sunLevel = GetSunLevel(context.home);
        var cloudLevel = GetCloudLevel(context.home);

        SetOutputValue(Sun, new Number(sunLevel), context);
        SetOutputValue(Clouds, new Number(cloudLevel), context);
        SetOutputValue(Brightness, new Number((float)Math.Clamp(sunLevel * (1f - cloudLevel * 0.75f), 0, 1)), context);
    }

    private float GetSunLevel(IHome home)
    {
        var sun = home.GetEntityState("sun.sun")!;
        var elevation = GetFloatAttribute(sun, "elevation");

        // Smoothstep from -6° (end of civil twilight) to 10° (full daylight)
        if (elevation <= -6f)
            return 0f;
        if (elevation >= 10f)
            return 1f;

        var t = (elevation + 6f) / 16f;
        return t * t * (3f - 2f * t);
    }

    private float GetCloudLevel(IHome home)
    {
        var weather = home.GetEntityState("weather.home");
        if (weather == null)
        {
            Log(LogLevel.Warning, "No weather state found");
            return 0;
        }

        // Prefer cloud_coverage attribute (0-100) when available
        if (weather.attributes.TryGetValue("cloud_coverage", out var coverage))
        {
            return GetFloatValue(coverage) / 100f;
        }

        // Fall back to weather state string
        switch (weather.state)
        {
            case "sunny":
            case "clear-night":
            case "windy":
                return 0f;

            case "partlycloudy":
            case "windy-variant":
                return 0.3f;

            case "cloudy":
                return 0.6f;

            case "rainy":
            case "snowy":
                return 0.7f;

            case "fog":
            case "hail":
            case "lightning":
            case "lightning-rainy":
            case "snowy-rainy":
            case "pouring":
            case "exceptional":
                return 0.9f;

            default:
                Log(LogLevel.Warning, "Unknown weather state: '{0}'", weather.state);
                return 0.5f;
        }
    }

    private static float GetFloatAttribute(EntityState entity, string key)
    {
        if (entity.attributes.TryGetValue(key, out var value))
            return GetFloatValue(value);
        return 0f;
    }

    private static float GetFloatValue(object value)
    {
        return value switch
        {
            double d => (float)d,
            float f => f,
            int i => i,
            _ => 0f
        };
    }
}