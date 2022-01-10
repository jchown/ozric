using OzricEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OzricEngineTests
{
    public class MessageTests
    {
        [Fact]
        public void lightSerializeRoundTripWorks()
        {
            var json = "{ \"id\": 2, \"type\": \"event\", \"event\": { \"event_type\": \"call_service\", \"data\": { \"domain\": \"light\", \"service\": \"turn_on\", \"service_data\": { \"brightness\": 89, \"hs_color\": [36, 100], \"entity_id\": [\"light.hue_color_spot_1\"]} }, \"origin\": \"LOCAL\", \"time_fired\": \"2022-01-04T23:02:03.732935+00:00\", \"context\": { \"id\": \"3a007cd20bc2f88d564e31dd5d82e2c2\", \"parent_id\": null, \"user_id\": \"27568fb5326f49428f78e8d219212733\"} } }";

            var message = JsonSerializer.Deserialize<ServerMessage>(json, Comms.JsonOptions);

            var ev = message as ServerEvent;

            Assert.Equal("call_service", ev.payload.event_type);

            var call = ev.payload as EventCallService;

            Assert.Equal(89, call.data.service_data.brightness);
        }


        [Fact]
        public void callServiceSerializeRoundTripWorks()
        {
            var entities = new List<string> { "light.hue_color_spot_1", "light.hue_color_spot_2", "light.hue_color_spot_3", "light.hue_color_spot_4" };
            var color = new List<float> { 0.18325046f, 0.4023088f };
            
            var a = new ClientCallService
            {
                domain = "light",
                service = "turn_on",
                target = new Attributes
                {
                    { "entity_id", entities },
                },
                service_data = new Attributes
                {
                    { "xy_color", color },
                    { "brightness", 50 }
                }
            };

            var json = JsonSerializer.Serialize(a);
            var b = JsonSerializer.Deserialize<ClientCallService>(json, Comms.JsonOptions);
            
            Assert.Equal(a, b);
        }
    }
}
