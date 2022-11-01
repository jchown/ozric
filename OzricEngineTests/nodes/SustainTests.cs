using System;
using OzricEngine.Values;
using OzricEngineTests;
using Xunit;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes
{
    public class SustainTests
    {
        [Fact]
        public void canSustainBinary()
        {
            // If the input is on for over 15 seconds, then
            // the output will stay on until it is off for 60 seconds
            
            var node = new BinarySustain("sus-1");
            node.sustainValue = true;
            node.sustainActivateSecs = 15;
            node.sustainDeactivateSecs = 60;
            
            DateTime now = DateTime.Now;
            
            //  Initial state is off

            var context = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Boolean(false), context);
            node.OnInit(context);
            Assert.Equal(false, node.GetOutputValue<Boolean>(BinarySustain.OUTPUT_NAME).value);

            //  A little later it goes on
            now = now.AddSeconds(10);
            AssertUpdateSustain(true, true, node, now);

            //  A little later it goes off (no sustain)
            now = now.AddSeconds(10);
            AssertUpdateSustain(false, false, node, now);

            //  A little later it goes on
            now = now.AddSeconds(10);
            AssertUpdateSustain(true, true, node, now);

            //  Much later it goes off (on is sustained)
            now = now.AddSeconds(20);
            AssertUpdateSustain(true, false, node, now);

            now = now.AddSeconds(20);
            AssertUpdateSustain(true, false, node, now);

            //  Much, much later it finally goes off
            now = now.AddSeconds(40);
            AssertUpdateSustain(false, false, node, now);
        }

        private static void AssertUpdateSustain(bool expectedOutput, bool input, BinarySustain node, DateTime now)
        {
            var context = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Boolean(input), context);
            node.OnUpdate(context);
            Assert.Equal(expectedOutput, node.GetOutputValue<Boolean>(BinarySustain.OUTPUT_NAME).value);
        }

        private static MockContext MockContextAtTime(DateTime now)
        {
            var home = new MockHome(now);
            var engine = new MockEngine(home);
            return new MockContext(engine);
        }
    }
}