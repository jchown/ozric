﻿using OzricEngine;
using System.Collections.Generic;
using System.Text.Json;
using OzricEngine.Nodes;
using OzricEngine.Values;
using Xunit;
using Xunit.Abstractions;
using ValueType = OzricEngine.Values.ValueType;

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
            assertSerializeRoundTripWorks(new List<Pin> { new("input", ValueType.Color, ColorRGB.RED) });
            assertSerializeRoundTripWorks(new InputSelector("a-node", "an-output"));
            assertSerializeRoundTripWorks(new Constant("red", ColorRGB.RED));
            assertSerializeRoundTripWorks(new DayPhases("work-times"));
            assertSerializeRoundTripWorks(new IfAll("all-right"));
            assertSerializeRoundTripWorks(new IfAny("any-left"));
            assertSerializeRoundTripWorks(new Light("Main Light", "hue-light-1"));
            assertSerializeRoundTripWorks(new ModeSwitch("bot-mode"));
            assertSerializeRoundTripWorks(new BinarySensor("Main Sensor", "sensor-01"));
            assertSerializeRoundTripWorks(new SkyBrightness());
            assertSerializeRoundTripWorks(new BinaryChoice("bot-colour", ValueType.Color));
        }
                
        [Fact]
        public void serializeRoundTripWorksWithInputs1()
        {
            var ifAll = new IfAll("all-right");
            ifAll.inputs.Add(new Pin("input-1", ValueType.Binary));
            ifAll.inputs.Add(new Pin("input-2", ValueType.Binary));
            assertSerializeRoundTripWorks(ifAll);
        }
                
        [Fact]
        public void serializeRoundTripWorksWithInputs2()
        {
            var ifAny = new IfAny("any-left");
            ifAny.inputs.Add(new Pin("input-1", ValueType.Binary));
            ifAny.inputs.Add(new Pin("input-2", ValueType.Binary));
            assertSerializeRoundTripWorks(ifAny);
        }

        private void assertSerializeRoundTripWorks(Node node)
        {
            assertSerializeRoundTripWorksGeneric(node);
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
