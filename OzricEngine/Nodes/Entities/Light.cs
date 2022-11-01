using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.Light)]
public class Light: EntityNode
{
    public override NodeType nodeType => NodeType.Light;

    public const string INPUT_NAME = "color";
    
    public ColourSwitchMode? colourSwitchMode { get; set; }

    public enum ColourSwitchMode
    {
        Automatic, Fast, TwoPhase
    }

    public Light(string id, string entityID) : base(id, entityID, new List<Pin> { new(INPUT_NAME, ValueType.Color) }, null)
    {
    }

    public override Task OnInit(Context context)
    {
        if (entityID == null)
        {
            Log(LogLevel.Error, $"Light state {id}: Entity ID is null");
            return Task.CompletedTask;
        }

        var state = context.home.GetEntityState(entityID);
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
        if (!HasInputValue(INPUT_NAME))
            return;
        
        var desired = GetInputValue<ColorValue>(INPUT_NAME);
        var command = GetCommand(desired, context.home);
        if (command == null)
            return;
        
        context.commands.Add(command, result =>
        {
            if (!result.success)
            {
                Log(LogLevel.Warning, "Service call failed ({0}) - {1}", result.error.code, result.error.message);
            }
        });
    }

    public ClientCallService? GetCommand(ColorValue desired, Home home)
    {
        var entityState = home.GetEntityState(entityID)!;
            
        if (!home.CanUpdateEntity(entityState))
            return null;
        
        var attributes = GetLightUpdate(desired, entityState, out var on, out var brightness, out var desiredOn, out var update, out var colorKey, out var colorValue);
        if (!update.update)
            return null;

//        entityState.LogLightState();
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

            if (GetColourSwitchMode() == ColourSwitchMode.Fast)
            {
                // Don't care about the current state, switch.

                callServices.service_data = new Attributes
                {
                    { "brightness", brightness },
                    { colorKey, colorValue }
                };
            }
            else
            {
                // Cheap lights (in my case, model tuyatec_zn9wyqtr_rh3040) don't like
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
        }
        else
            Log(LogLevel.Info, "call service {0}", callServices.service);

        return callServices;
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

    private LightAttributes GetLightUpdate(ColorValue desired, EntityState entityState, out bool on, out int brightness, out bool desiredOn, out UpdateReason update, out string? colorKey,
        out object? colorValue)
    {
        var attributes = entityState.LightAttributes;

        on = (entityState.state == "on") && attributes.brightness > 0;
        Log(LogLevel.Debug, "{0}.on = {1}", entityID, on);
        if (on)
            Log(LogLevel.Debug, "brightness = {0}", attributes.brightness);

        brightness = (int)(desired.brightness * 255 + 0.5f);
        desiredOn = brightness > 0;

        update = new UpdateReason();
        update.CheckEquals(desiredOn, on);
        if (on)
        {
            if (brightness == 0 || attributes.brightness == 0)
                update.CheckEquals(brightness, attributes.brightness);
            else
                update.CheckApprox(brightness, attributes.brightness, 3);
        }

        bool needsConversion = false;

        colorKey = null;
        colorValue = null;

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

        return attributes;
    }

    public ColourSwitchMode GetColourSwitchMode()
    {
        var mode = colourSwitchMode ?? ColourSwitchMode.Automatic;
        
        if (mode == ColourSwitchMode.Automatic)
            mode = GetColourSwitchModeAuto();

        return mode;
    }

    public ColourSwitchMode GetColourSwitchModeAuto()
    {
        return (entityID.ToLowerInvariant().Contains("hue") ? ColourSwitchMode.Fast : ColourSwitchMode.TwoPhase);
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
        var hs = desired.ToHS();

        var h = hs.h * 360;
        var s = hs.s;
        
        object colorValue = new List<float> { h, s };
        //brightness = (int)(Y * brightness);

        if (!update.Check(attributes.hs_color == null))
        {
            update.CheckApprox(attributes.hs_color[0], h, 5f);
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

    public static readonly string[] ATTRIBUTE_KEYS = { "brightness", "color_mode", "xy_color", "hs_color", "rgb_color" };
    
    public List<ColorMode> GetSupportedColorModes(Home home)
    {
        var entityState = home.GetEntityState(entityID)!;
        var attributes = entityState.LightAttributes;
        return attributes.supported_color_modes.Select(s =>
        {
            switch (s)
            {
                case "xy":
                    return ColorMode.XY;
                case "hs":
                    return ColorMode.HS;
                case "rgb":
                    return ColorMode.RGB;
                case "color_temp":
                    return ColorMode.Temp;
                
                default:
                    Log(LogLevel.Warning, "Unknown color mode: {0}", s);
                    return ColorMode.Unknown;
            }
        }).Where(s => s != ColorMode.Unknown).ToList();
    }

}