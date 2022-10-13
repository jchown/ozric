using OzricEngine;
using System.Collections.Generic;
using System.Text.Json;
using OzricEngine.Nodes;
using Xunit;
using OzricEngine.Values;
using Boolean = OzricEngine.Values.Boolean;

namespace OzricEngineTests
{
    public class ValuesTests
    {
        [Fact]
        public void serializeWorksTypedAndUntyped()
        {
            var value = ColorRGB.RED;
            var json1 = Json.Serialize(value, typeof(Value));
            var json2 = Json.Serialize(value, typeof(object));
            Assert.Equal(json1, json2);
        }

        [Fact (Skip = "No generic 'type' field, see also https://github.com/dotnet/runtime/issues/29960")]
        public void deserializeWorksTypedAndUntyped()
        {
            string json = "{\"value-type\":\"Color\",\"color-type\":\"RGB\",\"brightness\":1,\"rgb\":\"FF0000\"}";
            var value1 = Json.Deserialize<Value>(json);
            var value2 = Json.Deserialize<object>(json);
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void serializeRoundTripWorks()
        {
            assertSerializeRoundTripWorks<Value>(new ColorRGB(40f/255f, 128f/255f, 192f/255f, 0.333f));

            assertSerializeRoundTripWorks<Value>(new Scalar(3.14159f));
            
            assertSerializeRoundTripWorks<Value>(new Boolean(true));
            
            assertSerializeRoundTripWorks<Value>(new Mode("weekend"));
            
            assertSerializeRoundTripWorks<Value>(new ColorHS(0.333f, 0.666f, 1));
            
            assertSerializeRoundTripWorks<Value>(new ColorTemp(420, 0.666f));
        }

        [Fact]
        public void colorRGBRoundingIsSane()
        {
            Assert.Equal("FFFFFF", new ColorRGB(1f, 1f, 1f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("FFFFFF", new ColorRGB(0.999f, 0.999f, 0.999f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("808080", new ColorRGB(0.5f, 0.5f, 0.5f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("000000", new ColorRGB(0f, 0f, 0f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("000000", new ColorRGB(0.001f, 0.001f, 0.001f, 1f).ToRGB24().ToString("X6"));    
        }
        
        private void assertSerializeRoundTripWorks<T>(T value1) where T: notnull 
        {
            var json = Json.Serialize(value1, typeof(T));
            var value2 = Json.Deserialize<T>(json);
            
            Assert.Equal(value1, value2);
        }
    }
}
