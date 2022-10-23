using System;
using OzricEngine.Values;
using OzricEngineTests;
using Xunit;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes
{
    public class TweenTests
    {
        [Fact]
        public void canTweenScalar()
        {
            var node01 = new Tween("scalar-0-to-1", ValueType.Scalar);
            var node10 = new Tween("scalar-1-to-0", ValueType.Scalar);
            node01.speed = 0.25f;   // 25% per Tween.UPDATE_INTERVAL_SECS
            node10.speed = 0.25f;
            
            DateTime now = DateTime.Now;
            var context = MockContextAtTime(now);
            
            //  Init node01 = 1 -> 1
            node01.SetInputValue(Tween.INPUT_NAME, new Scalar(1), context);
            node01.OnInit(context);

            //  Init node10 = 0 -> 0
            node10.SetInputValue(Tween.INPUT_NAME, new Scalar(0), context);            
            node10.OnInit(context);
            
            //  Set node01 = 0 -> 1, node10 = 1 -> 0  
            node01.SetInputValue(Tween.INPUT_NAME, new Scalar(0), context);
            node10.SetInputValue(Tween.INPUT_NAME, new Scalar(1), context);            
            
            //  Move forward two "updates" = 75% of 75% = 0.5625
            now = now.AddSeconds(Tween.UPDATE_INTERVAL_SECS * 2);
            context = MockContextAtTime(now);
            node01.OnUpdate(context);
            node10.OnUpdate(context);
            
            Assert.Equal(1 - 0.5625f, node01.GetOutputValue<Scalar>(Tween.OUTPUT_NAME).value);
            Assert.Equal(0.5625f, node10.GetOutputValue<Scalar>(Tween.OUTPUT_NAME).value);
            
            //  Move forward 8 more "updates" = .75 ^ 10 = 0.05631351
            now = now.AddSeconds(Tween.UPDATE_INTERVAL_SECS * 8);
            context = MockContextAtTime(now);
            node01.OnUpdate(context);
            node10.OnUpdate(context);
            
            Assert.Equal(1 - 0.05631351f, node01.GetOutputValue<Scalar>(Tween.OUTPUT_NAME).value);
            Assert.Equal(0.05631351f, node10.GetOutputValue<Scalar>(Tween.OUTPUT_NAME).value);
        }

        private static MockContext MockContextAtTime(DateTime now)
        {
            var home = new MockHome(now);
            var engine = new MockEngine(home);
            return new MockContext(engine);
        }
    }
}