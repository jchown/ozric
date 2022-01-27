using OzricEngine;
using System.Collections.Generic;
using System.Text.Json;
using OzricEngine.logic;
using Xunit;

namespace OzricEngineTests
{
    public class NodeTests
    {
        [Fact]
        public void serializeRoundTripWorks()
        {
            assertSerializeRoundTripWorks(new Constant("red", ColorRGB.RED));
            assertSerializeRoundTripWorks(new DayPhases("work-times"));
            assertSerializeRoundTripWorks(new IfAll("all-right"));
            assertSerializeRoundTripWorks(new IfAny("any-left"));
            assertSerializeRoundTripWorks(new Light("Main Light", "hue-light-1"));
            assertSerializeRoundTripWorks(new ModeSwitch("bot-mode"));
            assertSerializeRoundTripWorks(new Sensor("Main Sensor", "sensor-01"));
            assertSerializeRoundTripWorks(new SkyBrightness());
            assertSerializeRoundTripWorks(new Switch("bot-colour", ValueType.Color));
        }
        
        private void assertSerializeRoundTripWorks(Node node1)
        {
            var json1 = Json.Serialize(node1);
            var node2 = Json.Deserialize<Node>(json1);
            var json2 = Json.Serialize(node2);
            Assert.Equal(json1, json2);
        }
    }
}
