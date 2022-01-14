using OzricEngine;
using System.Collections.Generic;
using System.Text.Json;
using OzricEngine.logic;
using Xunit;

namespace OzricEngineTests
{
    public class ValuesTests
    {
        [Fact]
        public void serializeRoundTripWorks()
        {
            assertSerializeRoundTripWorks(new Scalar(3.14159f));
            
            assertSerializeRoundTripWorks(new OnOff(true));
            
            assertSerializeRoundTripWorks(new Mode("weekend"));
            
            assertSerializeRoundTripWorks(new ColorHS(0.333f, 0.666f, 1));
            
            assertSerializeRoundTripWorks(new ColorRGB(0.25f, 0.5f, 0.75f, 0.333f));
            
            assertSerializeRoundTripWorks(new ColorTemp(420, 0.666f));
        }

        [Fact]
        public void colorRGBRoundingIsSane()
        {
            Assert.Equal("FFFFFF", new ColorRGB(1f, 1f, 1f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("FFFFFF", new ColorRGB(0.999f, 0.999f, 0.999f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("000000", new ColorRGB(0f, 0f, 0f, 1f).ToRGB24().ToString("X6"));    
            Assert.Equal("000000", new ColorRGB(0.001f, 0.001f, 0.001f, 1f).ToRGB24().ToString("X6"));    
        }
        
        private void assertSerializeRoundTripWorks(Value value1)
        {
            var json = Json.Serialize(value1);
           
            var value2 = Json.Deserialize<Value>(json);
            
            Assert.Equal(value1, value2);
        }
    }
}
