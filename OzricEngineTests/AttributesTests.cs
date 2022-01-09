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

            Assert.NotEqual(a, c);
            Assert.False(a == c);
        }
    }
}
