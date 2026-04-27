using System.Collections.Generic;
using Ozric.Engine.Messages;
using Xunit;

namespace Ozric.Engine.Tests
{
    public class AttributesTests
    {
        [Fact]
        public void equalityOperatorWorks()
        {
            var a = new Attributes { { "a", 1 }, { "b", "two" }, { "c", new List<string> { "111", "222", "333" } } };  
            var b = new Attributes { { "a", 1 }, { "b", "two" }, { "c", new List<string> { "111", "222", "333" } } };
            var c = new Attributes { { "a", 1 }, { "b", "two" }, { "c", new List<string> { "333", "222", "111" } } };
            
            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);

            Assert.NotEqual(a, c);
            Assert.False(a == c);
            Assert.True(a != c);
        }

        [Fact]
        public void serializeRoundTripWorks()
        {
            var a = new Attributes { { "a", 1 }, { "b", "two" }, { "c", new List<string> { "111", "222", "333" } } };
            var json = Json.Serialize(a);
            var b = Json.Deserialize<Attributes>(json);
            
            Assert.Equal(a, b);
        }
    }
}
