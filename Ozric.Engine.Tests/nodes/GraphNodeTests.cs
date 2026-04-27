using System.Collections.Generic;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Graph.Logic;
using Ozric.Engine.Nodes;
using Ozric.Engine.Values;
using Xunit;
using Xunit.Abstractions;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Engine.Tests
{
    public class GraphNodeTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        
        public GraphNodeTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public void simpleSerializeRoundTripWorks()
        {
            assertSerializeRoundTripWorks(new List<Pin> { new("input", ValueType.Color, ColorRGB.RED) });
            assertSerializeRoundTripWorks(new InputSelector("a-node", "an-output"));
            assertSerializeRoundTripWorks(new GraphConstant("red", ColorRGB.RED));
            assertSerializeRoundTripWorks(new GraphDayPhases("work-times"));
            assertSerializeRoundTripWorks(new GraphIfAll("all-right"));
            assertSerializeRoundTripWorks(new GraphIfAny("any-left"));
            assertSerializeRoundTripWorks(new GraphLight("Main Light", "hue-light-1"));
            assertSerializeRoundTripWorks(new GraphModeSwitch("bot-mode"));
            assertSerializeRoundTripWorks(new GraphBinarySensor("Main Sensor", "sensor-01"));
            assertSerializeRoundTripWorks(new GraphSkyBrightness());
            assertSerializeRoundTripWorks(new GraphBinaryChoice("bot-colour", ValueType.Color));
        }
                
        [Fact]
        public void serializeRoundTripWorksWithInputs1()
        {
            var ifAll = new GraphIfAll("all-right");
            ifAll.inputs.Add(new Pin("input-1", ValueType.Binary));
            ifAll.inputs.Add(new Pin("input-2", ValueType.Binary));
            assertSerializeRoundTripWorks(ifAll);
        }
                
        [Fact]
        public void serializeRoundTripWorksWithInputs2()
        {
            var ifAny = new GraphIfAny("any-left");
            ifAny.inputs.Add(new Pin("input-1", ValueType.Binary));
            ifAny.inputs.Add(new Pin("input-2", ValueType.Binary));
            assertSerializeRoundTripWorks(ifAny);
        }

        private void assertSerializeRoundTripWorks(GraphNode graphNode)
        {
            assertSerializeRoundTripWorksGeneric(graphNode);
        }
        
        private void assertSerializeRoundTripWorks(InputSelector inSelector)
        {
            assertSerializeRoundTripWorksGeneric(inSelector);
        }
        
        private void assertSerializeRoundTripWorks(List<Pin> pins)
        {
            assertSerializeRoundTripWorksGeneric(pins);
        }
        
        private void assertSerializeRoundTripWorksGeneric<T>(T t) where T: class
        {
            var json1 = Json.Serialize(t);
            var node2 = Json.Deserialize<T>(json1);
            var json2 = Json.Serialize(node2);

            Assert.Equal(json1, json2);
        }
    }
}
