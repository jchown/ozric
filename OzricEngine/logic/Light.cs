using System;
using System.Collections.Generic;
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
            var currentState = engine.home.Get(entityID);
            var attributes = currentState.LightAttributes;

            /*
            var callServices = new ClientCallService
            {
                domain = "light",
                service = (state == "on" ? "turn_off" : "turn_on"),
                target = new Dictionary<string, string>()
                {
                    { "entity_id", entityID }
                }
            };
            engine.comms.Send(callServices);
            */

            bool on = (currentState.state == "on");
            engine.Log($"{entityID}.on = {on}");
            if (on)
                engine.Log($"{entityID}.brightness = {attributes.brightness}");

            var desired = (GetInput("color").value as ColorValue);
            var brightness = ((int)(desired.brightness * 255));
            var desiredOn = brightness > 0;

            bool needsUpdate = desiredOn != on || (on && brightness != attributes.brightness);

            string colorMode = null;
            string colorKey = null;
            string colorValue = null;

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

                            needsUpdate |= attributes.hs_color[0] != hs.h && attributes.hs_color[1] != hs.s;
                        }

                        colorMode = "hs";
                        colorKey = "color_hs";
                        colorValue = $"{h},{s}";
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

                        colorMode = "rgb";
                        colorKey = "color_rgb";
                        colorValue = $"{r},{g},{b}";
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
                    callServices.service_data = new Dictionary<string, string>()
                    {
                        { "brightness", brightness.ToString()},
                        { "color_mode", colorMode },
                        { colorKey, colorValue }
                    };
                }

                await engine.comms.Send(callServices);
            }

            /*
            if (on)
            {

                switch (attributes.color_mode)
                {
                    case "color_temp":
                        engine.Log($"{entityID}.Color#temp = {attributes.color_temp}");
                        break;


                    case "rgb":
                        break;

                    case "rgbw":
                        engine.Log($"{entityID}.Color#rgbw = {attributes.rgb_color[0]},{attributes.rgb_color[1]},{attributes.rgb_color[2]},{attributes.rgb_color[3]}");
                        break;

                    case "rgbww":
                        engine.Log($"{entityID}.Color#rgbww = {attributes.rgb_color[0]},{attributes.rgb_color[1]},{attributes.rgb_color[2]},{attributes.rgb_color[3]},{attributes.rgb_color[4]}");
                        break;

                    default:
                        throw new Exception($"color_mode was not expected to be {attributes.color_mode} ({currentState.attributes.Get("color_mode") ?? "<not set>"})");
                }
            }
            */
        }
    }
}