using System;
using OzricEngine.Values;
using OzricEngineTests;
using Xunit;

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
            node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(false), context);
            node.OnInit(context);
            Assert.False(node.GetOutputValue<Binary>(BinarySustain.OUTPUT_NAME).value);

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

        [Fact]
        public void canSustainBinaryRigorous()
        {
            var node = new BinarySustain("sus-1");
            node.sustainValue = true;
            node.sustainActivateSecs = 15;
            node.sustainDeactivateSecs = 60;
            
            DateTime now = DateTime.Now;
            
            //  Initial state is off

            var context = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(false), context);
            node.OnInit(context);
            Assert.False(node.GetOutputValue<Binary>(BinarySustain.OUTPUT_NAME).value);
            
            //  No matter how many time we go off/on quickly it will go off after 

            now = AssertSustainRigorous(node, now, 5, 5, 5, false, false);
            
            //  No matter How many time we go off/on it will stay on after 

            now = AssertSustainRigorous(node, now, 20, 20, 20, false, true);
            
            //  Wait a bit longer and it finally goes off 
            
            now = now.AddSeconds(60);
            var contextOff = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(false), contextOff);
            node.OnUpdate(contextOff);
            Assert.False(node.GetOutputValue<Binary>(BinarySustain.OUTPUT_NAME).value);
        }
        
        /// <summary>
        /// Turn the input off and the on for the given seconds, then finally set the final state for the given time 
        /// </summary>

        private DateTime AssertSustainRigorous(BinarySustain node, DateTime now, int timeOff, int timeOn, int timeFinal, bool inputFinal, bool expectedOutput)
        {
            for (int i = 0; i < 10; i++)
            {
                now = now.AddSeconds(timeOff);

                var contextOff = MockContextAtTime(now);
                node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(false), contextOff);
                node.OnUpdate(contextOff);

                now = now.AddSeconds(timeOn);

                var contextOn = MockContextAtTime(now);
                node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(true), contextOn);
                node.OnUpdate(contextOn);
            }
            
            var contextPre = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(inputFinal), contextPre);
            node.OnUpdate(contextPre);
            
            now = now.AddSeconds(timeFinal);
            
            var contextPost = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(inputFinal), contextPost);
            node.OnUpdate(contextPost);
            Assert.Equal(expectedOutput, node.GetOutputValue<Binary>(BinarySustain.OUTPUT_NAME).value);
            
            return now;
        }

        private static void AssertUpdateSustain(bool expectedOutput, bool input, BinarySustain node, DateTime now)
        {
            var context = MockContextAtTime(now);
            node.SetInputValue(BinarySustain.INPUT_NAME, new Binary(input), context);
            node.OnUpdate(context);
            Assert.Equal(expectedOutput, node.GetOutputValue<Binary>(BinarySustain.OUTPUT_NAME).value);
        }

        private static MockContext MockContextAtTime(DateTime now)
        {
            var home = new MockHome(now);
            var engine = new MockEngine(home);
            return new MockContext(engine);
        }
    }
}