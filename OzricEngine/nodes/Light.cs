using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine.logic;

[TypeKey(NodeType.Light)]
public class Light: EntityNode
{
    public override NodeType nodeType => NodeType.Light;

    public const string INPUT_NAME = "color";

    [JsonIgnore]
    private int secondsToAllowOverrideByOthers { get; }

    public Light(string id, string entityID) : base(id, entityID, new List<Pin> { new(INPUT_NAME, ValueType.Color) }, null)
    {
        secondsToAllowOverrideByOthers = 10 * 60;
    }

    public override Task OnInit(Context context)
    {
        if (entityID == null)
        {
            Log(LogLevel.Error, $"Light state {id}: Entity ID is null");
            return Task.CompletedTask;
        }

        var state = context.engine.home.GetEntityState(entityID);
        if (state != null)
        {
            var attributes = state.LightAttributes;
            var brightness = attributes.brightness;

            if (brightness > 0)
            {
                switch (attributes.color_mode)
                {
                    case "temp":
                    {
                        Log(LogLevel.Info, "Initial color_temp = {0}, brightness: {1}", attributes.color_temp, brightness);
                        break;
                    }
                    case "xy":
                    {
                        Log(LogLevel.Info, "Initial xy_color = {0}, brightness: {1}", attributes.xy_color, brightness);
                        break;
                    }
                    case "hs":
                    {
                        Log(LogLevel.Info, "Initial hs_color = {0}, brightness: {1}", attributes.hs_color, brightness);
                        break;
                    }
                    case "rgb":
                    {
                        Log(LogLevel.Info, "Initial rgb_color = {0}, brightness: {1}", attributes.rgb_color, brightness);
                        break;
                    }
                }
            }
            else
            {
                Log(LogLevel.Info, "Initial brightness: {0}", brightness);
            }
        }
            
        UpdateValue(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    private void UpdateValue(Context context)
    {
        var engine = context.engine;
            
        var input = GetInput("color");
        if (input == null || input.value == null)
        {
            Log(LogLevel.Error, "{0} has no input called 'color'", entityID);
            return;
        }

        var entityState = engine.home.GetEntityState(entityID);
        if (GetSecondsSinceLastUpdated(engine) < MIN_UPDATE_INTERVAL_SECS)
            return;

        if (entityState.IsOverridden(engine.home.GetTime(), secondsToAllowOverrideByOthers))
        {
            Log(LogLevel.Warning, "{0} has been controlled by another service for {1:F1} seconds", entityID, entityState.GetNumSecondsSinceOverride(engine.home.GetTime()));
            return;
        }
            
        var attributes = entityState.LightAttributes;

        bool on = (entityState.state == "on") && attributes.brightness > 0;
        Log(LogLevel.Debug, "{0}.on = {1}", entityID, on);
        if (on)
            Log(LogLevel.Debug, "brightness = {0}", attributes.brightness);

        var desired = (input.value as ColorValue);
        if (desired == null)
            throw new Exception($"${entityID}.input[color] is a {input.value.GetType().Name}, not a {nameof(ColorValue)}");
            
        var brightness = ((int)(desired.brightness * 255 + 0.5f));
        var desiredOn = brightness > 0;

        var update = new UpdateReason();
        update.Check(desiredOn != on);
        update.Check(on && brightness != attributes.brightness);
            
        bool needsConversion = false;

        string colorKey = null;
        object colorValue = null;

        if (desiredOn)
        {
            switch (desired)
            {
                case ColorHS hs:
                {
                    colorValue = CalcColorValueHS(hs, attributes, update, ref needsConversion);
                    colorKey = "hs_color";
                    break;
                }

                case ColorXY xy:
                {
                    colorValue = CalcColorValueXY(xy, attributes, update, ref needsConversion);
                    colorKey = "xy_color";
                    break;
                }

                case ColorRGB rgb:
                {
                    colorValue = CalcColorValueRGB(rgb, attributes, update, ref needsConversion);
                    colorKey = "color_rgb";
                    break;
                }

                case ColorTemp temp:
                {
                    colorValue = CalcColorValueTemp(attributes, update, temp, ref needsConversion);
                    colorKey = "color_temp";
                    break;
                }

                default:
                {
                    throw new Exception($"Light {entityID} given color value of type {desired.GetType()}");
                }
            }
        }

        if (needsConversion)
        {
            //  Need to convert between colour spaces

            if (attributes.supported_color_modes.Contains("xy"))
            {
                colorValue = ConvertToXY(desired, update, attributes, brightness);
                colorKey = "xy_color";
            }
            /*
            if (attributes.supported_color_modes.Contains("rgb"))
            {
                
            }*/
            else if (attributes.supported_color_modes.Contains("hs"))
            {
                colorValue = ConvertToHS(desired, update, attributes, brightness);
                colorKey = "hs_color";
            }
            else
            {
                throw new Exception($"Don't know how to convert from {desired.GetType().Name} to a supported mode: [{attributes.supported_color_modes.Join(",")}]");
            }
        }

        if (update.update)
        {
            entityState.LogLightState(LogLevel.Info);
            Log(LogLevel.Info, "Update needed: {0}", update.reason);

            var callServices = new ClientCallService
            {
                domain = "light",
                service = desiredOn ? "turn_on" : "turn_off",
                target = new Attributes
                {
                    { "entity_id", new List<string> { entityID } }
                },
            };

            if (desiredOn)
            {
                if (colorKey == null || colorValue == null)
                    throw new Exception("Internal error: No color chosen");
                    
                // Cheap lights (in my case model tuyatec_zn9wyqtr_rh3040) don't like
                // changing brightness AND color mode at the same time
                    
                if (on && brightness == attributes.brightness || attributes.color_mode != GetColorMode(colorKey))
                {
                    //  Just change color
                        
                    callServices.service_data = new Attributes
                    {
                        { colorKey, colorValue }
                    };

                    Log(LogLevel.Info, "call service {0}, {1}={2}", callServices.service, colorKey, colorValue);
                }
                else if (on && brightness != attributes.brightness && attributes.color_mode == GetColorMode(colorKey))
                {
                    //  Just change brightness
                        
                    callServices.service_data = new Attributes
                    {
                        { "brightness", brightness },
                    };

                    Log(LogLevel.Info, "call service {0}, brightness {1}", callServices.service, brightness);
                }
                else
                {
                    //  Already in right color mode, so safe to change everything
                        
                    callServices.service_data = new Attributes
                    {
                        { "brightness", brightness },
                        { colorKey, colorValue }
                    };

                    Log(LogLevel.Info, "call service {0}, {1}={2}, brightness {3}", callServices.service, colorKey, colorValue, brightness);
                }
            }
            else
                Log(LogLevel.Info, "call service {0}", callServices.service);
                
            entityState.last_updated = engine.home.GetTime();
            entityState.lastUpdatedByOzric = engine.home.GetTime();

            context.commandSender.Add(callServices, result =>
            {
                if (result == null)
                {
                    Log(LogLevel.Warning, "Service call did not respond");
                    return;
                }

                if (!result.success)
                {
                    Log(LogLevel.Warning, "Service call failed ({0}) - {1}",  result.error.code, result.error.message);
                    return;
                }
                    
                /*
                //  Success, record the state (the actual color the light uses may be subtly different, if it's gamut doesn't support
                //  it, for example, so we assume that it is what we asked and then ignore the actual state changes from the device) 

                lock (entityState)
                {
                    if (desiredOn)
                    {
                        Log(LogLevel.Info, "Command succeeded, light is on");

                        entityState.state = "on";
                        entityState.attributes["brightness"] = brightness;

                        if (colorKey != null)
                        {
                            entityState.attributes[colorKey] = colorValue;
                            entityState.attributes["color_mode"] = GetColorMode(colorKey);
                        }
                    }
                    else
                    {
                        Log(LogLevel.Info, "Command succeeded, light is off");

                        entityState.state = "off";
                    }
                    
                    entityState.LogLightState();
                }*/
            });
        }
    }

    private static string GetColorMode(string colorKey)
    {
        return colorKey == "color_temp" ? "color_temp" : colorKey.Substring(0, colorKey.Length - 6);
    }

    private static object ConvertToXY(ColorValue desired, UpdateReason update, LightAttributes attributes, int brightness)
    {
        //  See https://gist.github.com/popcorn245/30afa0f98eea1c2fd34d

        desired.GetRGB(out var red, out var green, out var blue);

        red = (red > 0.04045f) ? MathF.Pow((red + 0.055f) / (1.0f + 0.055f), 2.4f) : (red / 12.92f);
        green = (green > 0.04045f) ? MathF.Pow((green + 0.055f) / (1.0f + 0.055f), 2.4f) : (green / 12.92f);
        blue = (blue > 0.04045f) ? MathF.Pow((blue + 0.055f) / (1.0f + 0.055f), 2.4f) : (blue / 12.92f);

        float X = red * 0.649926f + green * 0.103455f + blue * 0.197109f;
        float Y = red * 0.234327f + green * 0.743075f + blue * 0.022598f;
        float Z = red * 0.0000000f + green * 0.053077f + blue * 1.035763f;

        float x = X / (X + Y + Z);
        float y = Y / (X + Y + Z);

        object colorValue = new List<float> { x, y };
        //brightness = (int)(Y * brightness);

        if (!update.Check(attributes.xy_color == null))
        {
            update.CheckApprox(attributes.xy_color[0], x, 0.1f);
            update.CheckApprox(attributes.xy_color[1], y, 0.1f);
            update.Check(attributes.brightness != brightness);
        }

        return colorValue;
    }

        
    private static object ConvertToHS(ColorValue desired, UpdateReason update, LightAttributes attributes, int brightness)
    {
        desired.GetRGB(out var r, out var g, out var b);

        //  See https://www.cs.rit.edu/~ncs/color/t_convert.html

        float min = MathF.Min(r, MathF.Min(g, b));
        float max = MathF.Max(r, MathF.Max(g, b));

        var delta = max - min;

        float h, s;

        if (max == 0)
        {
            // r = g = b = 0		// s = 0, v is undefined
            s = 0;
            h = 0;
        }
        else
        {
            s = delta / max; // s

            if (r == max)
                h = (g - b) / delta; // between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta; // between cyan & yellow
            else
                h = 4 + (r - g) / delta; // between magenta & cyan

            h *= 60; // degrees
            if (h < 0)
                h += 360;
        }

        object colorValue = new List<float> { h, s };
        //brightness = (int)(Y * brightness);

        if (!update.Check(attributes.hs_color == null))
        {
            update.CheckApprox(attributes.hs_color[0], h, 0.5f);
            update.CheckApprox(attributes.hs_color[1], s, 0.5f);
            update.Check(attributes.brightness != brightness);
        }

        return colorValue;
    }

    private object CalcColorValueTemp(LightAttributes attributes, UpdateReason update, ColorTemp temp, ref bool needsConversion)
    {
        object colorValue;
        if (attributes.color_mode != "color_temp")
        {
            if (!attributes.supported_color_modes.Contains("color_temp"))
            {
                needsConversion = true;
            }
            else
            {
                update.Set("attributes.color_mode != \"color_temp\"");
            }
        }
        else
        {
            Log(LogLevel.Debug, "color#temp = {0}", attributes.color_temp);

            update.Check(attributes.color_temp != temp.temp);
        }

        colorValue = temp.temp;
        return colorValue;
    }

    private object CalcColorValueRGB(ColorRGB rgb, LightAttributes attributes, UpdateReason update, ref bool needsConversion)
    {
        object colorValue;
        int r = (int)(rgb.r * 255);
        int g = (int)(rgb.g * 255);
        int b = (int)(rgb.b * 255);

        if (attributes.color_mode != "rgb")
        {
            if (!attributes.supported_color_modes.Contains("rgb"))
            {
                needsConversion = true;
            }
            else
            {
                update.Set("attributes.color_mode != \"rgb\"");
            }
        }
        else
        {
            Log(LogLevel.Debug, "color#rgb = {0},{1},{2}", attributes.rgb_color[0], attributes.rgb_color[1], attributes.rgb_color[2]);

            update.Check(attributes.rgb_color[0] != r);
            update.Check(attributes.rgb_color[1] != g);
            update.Check(attributes.rgb_color[2] != b);
        }

        colorValue = new List<int> { r, g, b };
        return colorValue;
    }

    private object CalcColorValueXY(ColorXY xy, LightAttributes attributes, UpdateReason update, ref bool needsConversion)
    {
        object colorValue;
        float x = xy.x;
        float y = xy.y;

        if (attributes.color_mode != "xy")
        {
            if (!attributes.supported_color_modes.Contains("xy"))
            {
                needsConversion = true;
            }
            else
            {
                update.Set("attributes.color_mode != \"xy\"");
            }
        }
        else
        {
            Log(LogLevel.Debug, "color#xy = {0},{1}", attributes.xy_color[0], attributes.xy_color[1]);

            update.Check(attributes.xy_color[0] != x);
            update.Check(attributes.xy_color[1] != y);
        }

        colorValue = new List<float> { x, y };
        return colorValue;
    }

    private object CalcColorValueHS(ColorHS hs, LightAttributes attributes, UpdateReason update, ref bool needsConversion)
    {
        object colorValue;
        int h = (int)(hs.h * 360);
        int s = (int)(hs.s * 100);

        if (attributes.color_mode != "hs")
        {
            if (!attributes.supported_color_modes.Contains("hs"))
            {
                needsConversion = true;
            }
            else
            {
                update.Set("attributes.color_mode != \"hs\"");
            }
        }
        else
        {
            Log(LogLevel.Debug, "color#hs = {0},{1}", attributes.hs_color[0], attributes.hs_color[1]);

            update.CheckApprox(attributes.hs_color[0], h, 0.5f);
            update.CheckApprox(attributes.hs_color[1], s, 0.5f);
        }

        colorValue = new List<int> { h, s };
        return colorValue;
    }

    private const double MIN_UPDATE_INTERVAL_SECS = 0.5f;
    public static readonly string[] ATTRIBUTE_KEYS = { "brightness", "color_mode", "xy_color", "hs_color", "rgb_color" };
}

internal class UpdateReason
{
    public bool update;
    public string reason;

    public void CheckApprox(float v0, float v1, float epsilon, [CallerArgumentExpression("v0")] string? v0s = null, [CallerArgumentExpression("v1")] string? v1s = null)
    {
        if (!update && Math.Abs(v0 - v1) > epsilon)
        {
            update = true;
            reason = $"{v0s} ({v0:F2}) !~= {v1s} ({v1:F2}), ε={epsilon:F2}";
        }
    }

    public bool Check(bool condition, [CallerArgumentExpression("condition")] string? conditionString = null)
    {
        if (!update && condition)
        {
            update = true;
            reason = conditionString;
        }

        return update;
    }

    public void Set(string reason)
    {
        Check(true, reason);
    }
}
    
