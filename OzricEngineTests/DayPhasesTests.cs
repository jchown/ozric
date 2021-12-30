using System;
using System.Collections.Generic;
using OzricEngineTests;
using Xunit;

namespace OzricEngine.logic
{
    public class DayPhasesTests
    {
        [Fact]
        public void canSplitDayInToMorningAndEvening()
        {
            var node = new DayPhases("am_pm");
            node.outputs.Add(new Pin("morning", new OnOff()));
            node.phases.Add(new DayPhases.PhaseStart( new Dictionary<string, object> { { "morning", new OnOff(true) } }, DayPhases.SunPhase.Midnight));
            node.phases.Add(new DayPhases.PhaseStart( new Dictionary<string, object> { { "morning", new OnOff() } }, DayPhases.SunPhase.Noon));

            var homePM = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_morning");
            var enginePM = new MockEngine(homePM);
            node.OnInit(enginePM);
            Assert.Equal(new OnOff(false), node.GetOutputOnOff("morning"));

            var homeAM = new MockHome(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"), "sun_morning");
            var engineAM = new MockEngine(homeAM);
            node.OnInit(engineAM);
            Assert.Equal(new OnOff(true), node.GetOutputOnOff("morning"));
        }

        [Fact]
        public void canEvaluatePhaseCorrectly()
        {
            var kitchenLightColours = new DayPhases("kitchen-lights-colours");
            kitchenLightColours.AddOutputValue("colour-on", new Colour());
            kitchenLightColours.AddOutputValue("colour-off", new Colour());
            kitchenLightColours.phases.Add(new DayPhases.PhaseStart(new Dictionary<string, object> { { "colour-on", dullWarmWhite }, { "colour-off", veryDullOrange }  }, DayPhases.SunPhase.Midnight));
            kitchenLightColours.phases.Add(new DayPhases.PhaseStart(new Dictionary<string, object> { { "colour-on", brightWhite }, { "colour-off", dullOrange }  }, DayPhases.SunPhase.Midnight, 6 * 60 * 60));
            kitchenLightColours.phases.Add(new DayPhases.PhaseStart(new Dictionary<string, object> { { "colour-on", warmWhite }, { "colour-off", dullOrange }  }, DayPhases.SunPhase.Setting, -30 * 60));
        }
    }
}