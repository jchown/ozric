using System;
using OzricEngine.Nodes;
using OzricEngine.Values;
using Xunit;

namespace OzricEngineTests
{
    public class TestSkyBrightness
    {
        [Fact]
        public void skyBrightnessIsZeroAfterDusk()
        {
            SkyBrightness skyBrightness = new SkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);
            skyBrightness.OnInit(context);
            
            Assert.Equal(0f, skyBrightness.GetOutputValue<Number>(SkyBrightness.brightness).value);
        }
        
        [Fact]
        public void skyBrightnessRisesAfterDawn()
        {
            var morning = DateTime.Parse("2021-11-30T07:51:25.459551+00:00");
            SkyBrightness skyBrightness = new SkyBrightness();
            var home = new MockHome(morning, "sun_morning", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);
            Assert.Equal(0.71f, skyBrightness.GetOutputValue<Number>(SkyBrightness.brightness).value, 2);

            home.SetTime(morning.AddMinutes(10));

            skyBrightness.OnUpdate(context);
            Assert.Equal(0.95f, skyBrightness.GetOutputValue<Number>(SkyBrightness.brightness).value, 2);
        }
        
        [Fact]
        public void skyBrightnessIsOneAtNoon()
        {
            SkyBrightness skyBrightness = new SkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-30T12:21:25.459551+00:00"), "sun_daytime", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);
            
            Assert.Equal(1f, skyBrightness.GetOutputValue<Number>(SkyBrightness.brightness).value);
        }
        
        [Fact]
        public void skyBrightnessIsReducedIfCloudy()
        {
            SkyBrightness skyBrightness = new SkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-30T12:21:25.459551+00:00"), "sun_daytime", "weather_cloudy");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);
            
            Assert.Equal(0.6f, skyBrightness.GetOutputValue<Number>(SkyBrightness.brightness).value);
        }
    }
}