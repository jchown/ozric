using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public class Light: Node
    {
        private string entityID { get; set; }
        private int secondsToAllowOverrideByOthers { get; set; }

        public Light(string id, string entityID) : base(id, new List<Pin> { new Pin("color", ValueType.Color) }, null)
        {
            this.entityID = entityID;
            this.secondsToAllowOverrideByOthers = 10 * 60;
        }

        public override async Task OnInit(Engine engine)
        {
            await UpdateValue(engine);
        }

        public override async Task OnUpdate(Engine engine)
        {
            await UpdateValue(engine);
        }

        private async Task UpdateValue(Engine engine)
        {
            var input = GetInput("color");
            if (input == null || input.value == null)
            {
                Log(LogLevel.Error, "{0} has no input called 'color'", entityID);
                return;
            }

            var currentState = engine.home.Get(entityID);

            if (currentState.IsOverridden(engine.home.GetTime(), secondsToAllowOverrideByOthers))
            {
                Log(LogLevel.Error, "{0} is being controlled by another service", entityID);
                return;
            }
            
            var attributes = currentState.LightAttributes;

            bool on = (currentState.state == "on");
            Log(LogLevel.Debug, "{0}.on = {1}", entityID, on);
            if (on)
                Log(LogLevel.Debug, "brightness = {0}", attributes.brightness);

            var desired = (input.value as ColorValue);
            if (desired == null)
                throw new Exception($"${entityID}.input[color] is a {input.value.GetType().Name}, not a {nameof(ColorValue)}");
            
            var brightness = ((int)(desired.brightness * 255));
            var desiredOn = brightness > 0;

            bool needsUpdate = desiredOn != on || (on && brightness != attributes.brightness);
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
                                needsUpdate = true;
                            }
                        }
                        else
                        {
                            Log(LogLevel.Debug, "color#hs = {0},{1}", attributes.hs_color[0], attributes.hs_color[1]);

                            needsUpdate |= attributes.hs_color[0] != h || attributes.hs_color[1] != s;
                        }

                        colorKey = "hs_color";
                        colorValue = new List<int> { h, s };
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
                                needsUpdate = true;
                            }
                        }
                        else
                        {
                            Log(LogLevel.Debug, "color#rgb = {0},{1},{2}", attributes.rgb_color[0], attributes.rgb_color[1], attributes.rgb_color[2]);

                            needsUpdate |= (attributes.rgb_color[0] != r) || (attributes.rgb_color[1] != g) || (attributes.rgb_color[2] != b);
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
                                needsUpdate = true;
                            }
                        }
                        else
                        {
                            Log(LogLevel.Debug, "color#temp = {0}", attributes.color_temp);

                            needsUpdate |= (attributes.color_temp != temp.t);
                        }

                        colorKey = "color_temp";
                        colorValue = temp.t;
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

                    needsUpdate = attributes.xy_color == null || (attributes.xy_color[0] != x) || (attributes.xy_color[1] != y) || (attributes.brightness != brightness);
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

            if (needsUpdate)
            {
                var callServices = new ClientCallService
                {
                    domain = "light",
                    service = desiredOn ? "turn_on" : "turn_off",
                    target = new Dictionary<string, string>()
                    {
                        { "entity_id", entityID }
                    }
                };

                if (desiredOn)
                {
                    if (colorKey == null || colorValue == null)
                        throw new Exception("Internal error: No color chosen");
                    
                    callServices.service_data = new Dictionary<string, object>()
                    {
                        { "brightness", brightness},
                        { colorKey, colorValue }
                    };
                }
                
                currentState.lastUpdatedByOzric = engine.home.GetTime();

                var result = await engine.comms.SendCommand(callServices, COMMAND_TIMEOUT_MS);
                if (result == null)
                {
                    Log(LogLevel.Warning, "Entity failed to respond");
                }
                else if (!result.success)
                {
                    Log(LogLevel.Warning, "Entity failed to update: {0} - {1}", result.error.code, result.error.message);
                }
                else
                {
                    if (desiredOn)
                    {
                        currentState.attributes["brightness"] = brightness;
                        currentState.attributes[colorKey] = colorValue;
                    }
                    else
                    {
                        currentState.attributes["brightness"] = 0;
                    }
                }
            }
        }

        private const int COMMAND_TIMEOUT_MS = 5000;
    }
}