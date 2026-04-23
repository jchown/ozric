using System;
using Ozric.Engine.Extensions;
using Xunit;

namespace Ozric.Engine.Tests.Extensions;

public class TypeExtTests
{
    [Fact]
    public void CanTestForInterfaces()
    {
        // Basic case
        Assert.True(typeof(int).Implements(typeof(IComparable)));
        
        // Make sure it isn't commutative
        Assert.False(typeof(IComparable).Implements(typeof(int)));
        
        // Make sure implements != derived from
        Assert.False(typeof(ISpanFormattable).Implements(typeof(IFormattable)));
    }
}
