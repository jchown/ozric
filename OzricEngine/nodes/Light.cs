using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public class Light: Node
    {
        private readonly string entityID;

        public Light(string id, string entityID) : base(id, new List<Pin> { new Pin("color", ValueType.Color) }, null)
        {
            this.entityID = entityID;
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
                engine.Log($"{entityID} has no input called 'color'");
                return;
            }

            var currentState = engine.home.Get(entityID);
            var attributes = currentState.LightAttributes;

            bool on = (currentState.state == "on");
            engine.Log($"{entityID}.on = {on}");
            if (on)
                engine.Log($"{entityID}.brightness = {attributes.brightness}");

            var desired = (input.value as ColorValue);
            if (desired == null)
                throw new Exception($"${entityID}.input[color] is a {input.value.GetType().Name}, not a {nameof(ColorValue)}");
            
            var brightness = ((int)(desired.brightness * 255));
            var desiredOn = brightness > 0;

            bool needsUpdate = desiredOn != on || (on && brightness != attributes.brightness);

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
                                throw new Exception($"Light {entityID} does not support HS color mode");

                            needsUpdate = true;
                        }
                        else
                        {
                            engine.Log($"{entityID}.Color#hs = {attributes.hs_color[0]},{attributes.hs_color[1]}");

                            needsUpdate |= attributes.hs_color[0] != h && attributes.hs_color[1] != s;
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
                                throw new Exception($"Light {entityID} does not support RGB color mode");

                            needsUpdate = true;
                        }
                        else
                        {
                            engine.Log($"{entityID}.Color#rgb = {attributes.rgb_color[0]},{attributes.rgb_color[1]},{attributes.rgb_color[2]}");

                            needsUpdate |= (attributes.rgb_color[0] != r) && (attributes.rgb_color[1] != g) && (attributes.rgb_color[2] != b);
                        }

                        colorKey = "color_rgb";
                        colorValue = new List<int> { r, g, b };
                        break;
                    }

                    default:
                    {
                        throw new Exception($"Light {entityID} given color value of type {desired.GetType()}");
                    }
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
                    callServices.service_data = new Dictionary<string, object>()
                    {
                        { "brightness", brightness},
                        { colorKey, colorValue }
                    };
                }

                var result = await engine.comms.SendCommand(callServices, COMMAND_TIMEOUT_MS);
                if (result == null)
                {
                    engine.Log($"Light {entityID} failed to respond");
                }
                else if (!result.success)
                {
                    engine.Log($"Light {entityID} failed to update: {result.error.code} - {result.error.message}");
                }
                else
                {
                    currentState.attributes["brightness"] = brightness;
                    currentState.attributes[colorKey] = colorValue;
                }
            }
        }

        private const int COMMAND_TIMEOUT_MS = 5000;
    }
}