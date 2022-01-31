using OzricEngine;
using System.Collections.Generic;
using System.Text.Json;
using OzricEngine.logic;
using Xunit;
using Xunit.Abstractions;

namespace OzricEngineTests
{
    public class NodeTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        
        public NodeTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public void simpleSerializeRoundTripWorks()
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

                
        [Fact]
        public void serializeRoundTripWorksWithInputs()
        {
            var ifAll = new IfAll("all-right");
            ifAll.inputs.Add(new Pin("input-1", ValueType.OnOff));
            ifAll.inputs.Add(new Pin("input-2", ValueType.OnOff));
            assertSerializeRoundTripWorks(ifAll);
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
