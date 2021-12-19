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
            node.OnInit(homePM);
            Assert.Equal(new OnOff(false), node.GetOutputOnOff("morning"));

            var homeAM = new MockHome(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"), "sun_morning");
            node.OnInit(homeAM);
            Assert.Equal(new OnOff(true), node.GetOutputOnOff("morning"));
        }
    }
}