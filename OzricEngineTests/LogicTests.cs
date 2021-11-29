using System;
using OzricEngine.logic;
using Xunit;

namespace OzricEngineTests
{
    public class LogicTests
    {
        [Fact]
        public void skyBrightnessIsZeroAfterDusk()
        {
            SkyBrightness skyBrightness = new SkyBrightness();
            MockHome home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening");
            skyBrightness.OnInit(home);
            
            Assert.Equal(0f, skyBrightness.value);
        }
        
        [Fact]
        public void skyBrightnessRisesAfterDawn()
        {
            var morning = DateTime.Parse("2021-11-30T07:51:25.459551+00:00");
            SkyBrightness skyBrightness = new SkyBrightness();
            MockHome home = new MockHome(morning, "sun_morning");

            skyBrightness.OnInit(home);
            Assert.Equal(0.57f, skyBrightness.value, 2);

            home.SetTime(morning.AddMinutes(10));

            skyBrightness.OnUpdate(home);
            Assert.Equal(0.76f, skyBrightness.value, 2);
        }
    }
}