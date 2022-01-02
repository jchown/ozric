using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public class Light: Node
    {
        private readonly string entityID;

        public Light(string id, string entityID) : base(id, new List<Pin> { new Pin("on", new OnOff()), new Pin("color", new ColorRGB()) }, null)
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

            var value = (GetInput("on").value as OnOff).value;
            if (value != on)
            {
                // Turn off
                
                var callServices = new ClientCallService
                {
                    domain = "light",
                    service = on ? "turn_off" : "turn_on",
                    target = new Dictionary<string, string>()
                    {
                        { "entity_id", entityID }
                    }
                };
                await engine.comms.Send(callServices);
            }

            if (on)
            {
                engine.Log($"{entityID}.brightness = {attributes.brightness}");

                switch (attributes.color_mode)
                {
                    case "color_temp":
                        engine.Log($"{entityID}.Color#temp = {attributes.color_temp}");
                        break;

                    case "hs":
                        engine.Log($"{entityID}.Color#hs = {attributes.hs_color[0]},{attributes.hs_color[1]}");
                        break;

                    case "rgb":
                        engine.Log($"{entityID}.Color#rgb = {attributes.rgb_color[0]},{attributes.rgb_color[1]},{attributes.rgb_color[2]}");
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
        }
    }
}