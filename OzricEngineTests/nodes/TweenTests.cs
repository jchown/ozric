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
        public void tweenMathChecksOut()
        {
            Assert.Equal(0.1f, Tween.Lerp(0, 1, 0.1f));
            Assert.Equal(0.9f, Tween.Lerp(1, 0, 0.1f));
            Assert.Equal(0.5f, Tween.Lerp(0.25f, 0.75f, 0.5f));
            Assert.Equal(0.5f, Tween.Lerp(0.75f, 0.25f, 0.5f));
            
            Assert.True(ApproxEquals(0.75f, Tween.GetLerpRate(Tween.UPDATE_INTERVAL_SECS, 0.25f)));
            Assert.True(ApproxEquals(0.75f * 0.75f, Tween.GetLerpRate(Tween.UPDATE_INTERVAL_SECS * 2, 0.25f)));
            Assert.True(ApproxEquals(0.75f * 0.75f * 0.75f, Tween.GetLerpRate(Tween.UPDATE_INTERVAL_SECS * 3, 0.25f)));
            Assert.True(ApproxEquals(0.75f * 0.75f * 0.75f * 0.75f, Tween.GetLerpRate(Tween.UPDATE_INTERVAL_SECS * 4, 0.25f)));
            Assert.True(ApproxEquals(MathF.Pow(0.75f, 8), Tween.GetLerpRate(Tween.UPDATE_INTERVAL_SECS * 8, 0.25f)));
            Assert.True(ApproxEquals(MathF.Pow(0.75f, 10), Tween.GetLerpRate(Tween.UPDATE_INTERVAL_SECS * 10, 0.25f)));
        }

        [Fact]
        public void canTweenScalar()
        {
            var node01 = new Tween("scalar-0-to-1", ValueType.Number);
            var node10 = new Tween("scalar-1-to-0", ValueType.Number);
            node01.speed = 0.25f;   // 25% per Tween.UPDATE_INTERVAL_SECS
            node10.speed = 0.25f;
            
            DateTime now = DateTime.Now;
            var context = MockContextAtTime(now);
            
            //  node01 = 0 -> 1
            node01.SetInputValue(Tween.INPUT_NAME, new Number(1), context);
            node01.OnInit(context);
            node01.SetInputValue(Tween.INPUT_NAME, new Number(0), context);

            //  node10 = 1 -> 0
            node10.SetInputValue(Tween.INPUT_NAME, new Number(0), context);            
            node10.OnInit(context);
            node10.SetInputValue(Tween.INPUT_NAME, new Number(1), context);            
            
            //  Move forward one "update" = 25% towards/away from 1
            now = now.AddSeconds(Tween.UPDATE_INTERVAL_SECS);
            context = MockContextAtTime(now);
            node01.OnUpdate(context);
            node10.OnUpdate(context);
            
            Assert.Equal(0.25f, node01.GetOutputValue<Number>(Tween.OUTPUT_NAME).value);
            Assert.Equal(0.75f, node10.GetOutputValue<Number>(Tween.OUTPUT_NAME).value);
            
            //  Move forward another "update" = 43.75%
            now = now.AddSeconds(Tween.UPDATE_INTERVAL_SECS);
            context = MockContextAtTime(now);
            node01.OnUpdate(context);
            node10.OnUpdate(context);
            
            Assert.Equal(0.4375f, node01.GetOutputValue<Number>(Tween.OUTPUT_NAME).value);
            Assert.Equal(0.5625f, node10.GetOutputValue<Number>(Tween.OUTPUT_NAME).value);
            
            //  Move forward 8 more "updates" = 94.36%
            now = now.AddSeconds(Tween.UPDATE_INTERVAL_SECS * 8);
            context = MockContextAtTime(now);
            node01.OnUpdate(context);
            node10.OnUpdate(context);

            Assert.True(ApproxEquals(1 - 0.05631351f, node01.GetOutputValue<Number>(Tween.OUTPUT_NAME).value));
            Assert.True(ApproxEquals(0.05631351f, node10.GetOutputValue<Number>(Tween.OUTPUT_NAME).value));
        }

        private static MockContext MockContextAtTime(DateTime now)
        {
            var home = new MockHome(now);
            var engine = new MockEngine(home);
            return new MockContext(engine);
        }
        
        private bool ApproxEquals(float a, float b)
        {
            return MathF.Abs(a - b) < 0.0001f;
        }
    }
}