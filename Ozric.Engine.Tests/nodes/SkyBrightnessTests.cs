using System;
using Ozric.Engine.Nodes;
using Ozric.Engine.Values;
using Xunit;

namespace Ozric.Engine.Tests
{
    public class TestSkyBrightness
    {
        [Fact]
        public void skyBrightnessIsZeroAtNight()
        {
            GraphSkyBrightness skyBrightness = new GraphSkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);
            skyBrightness.OnInit(context);

            Assert.Equal(0f, skyBrightness.GetOutputValue<Number>(GraphSkyBrightness.Brightness).value);
        }

        [Fact]
        public void skyBrightnessIsIntermediateAtLowSun()
        {
            // sun_morning has elevation 2.0: t = (2+6)/16 = 0.5, smoothstep(0.5) = 0.5
            GraphSkyBrightness skyBrightness = new GraphSkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-30T07:51:25.459551+00:00"), "sun_morning", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);
            Assert.Equal(0.5f, skyBrightness.GetOutputValue<Number>(GraphSkyBrightness.Brightness).value, 2);
        }

        [Fact]
        public void skyBrightnessIsOneAtFullDay()
        {
            // sun_daytime has elevation 15.0: above 10° threshold, sunLevel = 1.0
            GraphSkyBrightness skyBrightness = new GraphSkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-30T12:21:25.459551+00:00"), "sun_daytime", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);

            Assert.Equal(1f, skyBrightness.GetOutputValue<Number>(GraphSkyBrightness.Brightness).value);
        }

        [Fact]
        public void skyBrightnessIsReducedByCloudCoverage()
        {
            // sun_daytime (elevation 15.0) + weather_cloudy (cloud_coverage=80)
            // cloudLevel = 0.8, brightness = 1.0 * (1 - 0.8 * 0.75) = 0.4
            GraphSkyBrightness skyBrightness = new GraphSkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-30T12:21:25.459551+00:00"), "sun_daytime", "weather_cloudy");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);

            Assert.Equal(0.4f, skyBrightness.GetOutputValue<Number>(GraphSkyBrightness.Brightness).value, 2);
        }

        [Fact]
        public void skyBrightnessFallsBackToStateString()
        {
            // sun_daytime (elevation 15.0) + weather_sunny (no cloud_coverage attribute)
            // Falls back to state "sunny" → cloudLevel = 0, brightness = 1.0
            GraphSkyBrightness skyBrightness = new GraphSkyBrightness();
            var home = new MockHome(DateTime.Parse("2021-11-30T12:21:25.459551+00:00"), "sun_daytime", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            skyBrightness.OnInit(context);

            Assert.Equal(1f, skyBrightness.GetOutputValue<Number>(GraphSkyBrightness.Brightness).value);
        }
    }
}
