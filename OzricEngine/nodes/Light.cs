using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.Light)]
    public class Light: EntityNode
    {
        public override NodeType nodeType => NodeType.Light;

        [JsonIgnore]
        private int secondsToAllowOverrideByOthers { get; }

        public Light(string id, string entityID) : base(id, entityID, new List<Pin> { new("color", ValueType.Color) }, null)
        {
            secondsToAllowOverrideByOthers = 10 * 60;
        }

        public override Task OnInit(Context context)
        {
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
            
            //if (GetSecondsSinceLastUpdated(engine) < MIN_UPDATE_INTERVAL_SECS)
            //    return;
            
            var input = GetInput("color");
            if (input == null || input.value == null)
            {
                Log(LogLevel.Error, "{0} has no input called 'color'", entityID);
                return;
            }

            var currentState = engine.home.GetEntityState(entityID);

            if (currentState.IsOverridden(engine.home.GetTime(), secondsToAllowOverrideByOthers))
            {
                Log(LogLevel.Warning, "{0} has been controlled by another service for {1:F1} seconds", entityID, currentState.GetNumSecondsSinceOverride(engine.home.GetTime()));
                return;
            }
            
            var attributes = currentState.LightAttributes;

            bool on = (currentState.state == "on") && attributes.brightness > 0;
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

                            update.Check(attributes.hs_color[0] != h);
                            update.Check(attributes.hs_color[1] != s);
                        }

                        colorKey = "hs_color";
                        colorValue = new List<int> { h, s };
                        break;
                    }

                    case ColorXY xy:
                    {
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

                        colorKey = "xy_color";
                        colorValue = new List<float> { x, y };
                        break;
                    }

                    case ColorRGB rgb:
                    {
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

                        colorKey = "color_rgb";
                        colorValue = new List<int> { r, g, b };
                        break;
                    }

                    case ColorTemp temp:
                    {
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

                        colorKey = "color_temp";
                        colorValue = temp.temp;
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

                    colorKey = "xy_color";
                    colorValue = new List<float> { x, y };
                    //brightness = (int)(Y * brightness);

                    if (!update.Check(attributes.xy_color == null))
                    {
                        update.Check(attributes.xy_color[0] != x);
                        update.Check(attributes.xy_color[1] != y);
                        update.Check(attributes.brightness != brightness);
                    }
                }
                /*
                if (attributes.supported_color_modes.Contains("rgb"))
                {
                    
                }
                else if (attributes.supported_color_modes.Contains("hs"))
                {
                    
                }*/
                else
                {
                    throw new Exception($"Don't know how to convert from {desired.GetType().Name} to a supported mode: [{attributes.supported_color_modes.Join(",")}]");
                }
            }

            if (update.update)
            {
                Log(LogLevel.Info, "Updated needed: {0}", update.reason);

                var callServices = new ClientCallService
                {
                    domain = "light",
                    service = desiredOn ? "turn_on" : "turn_off",
                    target = new Attributes()
                    {
                        { "entity_id", new List<string> { entityID } }
                    },
                };

                if (desiredOn)
                {
                    if (colorKey == null || colorValue == null)
                        throw new Exception("Internal error: No color chosen");
                    
                    callServices.service_data = new Attributes()
                    {
                        { "brightness", brightness},
                        { colorKey, colorValue }
                    };
                    
                    Log(LogLevel.Info, "call service {0}, {1}={2}, brightness {3}", callServices.service, colorKey, colorValue, brightness);
                }
                else
                    Log(LogLevel.Info, "call service {0}", callServices.service);
                
                currentState.lastUpdatedByOzric = engine.home.GetTime();

                context.commandSender.Add(callServices, (result) =>
                {
                    if (result == null)
                    {
                        Log(LogLevel.Warning, "Service call did not ");
                        return;
                    }

                    if (!result.success)
                    {
                        Log(LogLevel.Warning, "Service call failed ({1}) - {2}",  result.error.code, result.error.message);
                        return;
                    }
                    
                    //  Success, record the state (the actual color the light uses may be subtly different, if it's gamut doesn't support
                    //  it, for example, so we assume that it is what we asked and then ignore the actual state changes from the device) 

                    if (desiredOn)
                    {
                        currentState.attributes["brightness"] = brightness;
                        currentState.attributes[colorKey] = colorValue;
                        currentState.attributes["color_mode"] = colorKey == "color_temp" ? "color_temp" : colorKey.Substring(0, colorKey.Length - 6);
                    }
                    else
                    {
                        currentState.attributes["brightness"] = 0;
                    }
                });
            }
        }

        private const double MIN_UPDATE_INTERVAL_SECS = 3;
    }

    internal struct UpdateReason
    {
        public bool update;
        public string reason;

        public bool Check(bool condition, [CallerArgumentExpression("condition")] string reason = null)
        {
            if (!update && condition)
            {
                update = true;
                this.reason = reason;
            }

            return update;
        }

        public void Set(string reason)
        {
            Check(true, reason);
        }
    }
}
