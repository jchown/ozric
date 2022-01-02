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
            node.phases.Add(new DayPhases.PhaseStart( new Dictionary<string, Value> { { "morning", new OnOff(true) } }, DayPhases.SunPhase.Midnight));
            node.phases.Add(new DayPhases.PhaseStart( new Dictionary<string, Value> { { "morning", new OnOff() } }, DayPhases.SunPhase.Noon));

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
            var node = new DayPhases("Color-phases");
            node.AddOutputValue("Color", new ColorRGB());
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(1,0,0,0) }  }, DayPhases.SunPhase.Midnight));
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(0,1,0,0) }  }, DayPhases.SunPhase.Midnight, + 6 * 60 * 60));
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(0,0,1,0) }  }, DayPhases.SunPhase.Noon));
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(0,0,0,1) }  }, DayPhases.SunPhase.Midnight, - 6 * 60 * 60));
            
            var home = new MockHome(DateTime.Parse("2021-11-29T03:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(home);
            node.OnInit(engine);

            Assert.Equal(new ColorRGB(1,0,0,0), node.GetOutputColor("Color"));

            home.SetTime(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"));
            node.OnUpdate(engine);
            Assert.Equal(new ColorRGB(0, 1, 0, 0), node.GetOutputColor("Color"));

            home.SetTime(DateTime.Parse("2021-11-29T13:21:25.459551+00:00"));
            node.OnUpdate(engine);
            Assert.Equal(new ColorRGB(0, 0, 1, 0), node.GetOutputColor("Color"));

            home.SetTime(DateTime.Parse("2021-11-29T23:21:25.459551+00:00"));
            node.OnUpdate(engine);
            Assert.Equal(new ColorRGB(0, 0, 0, 1), node.GetOutputColor("Color"));
        }
    }
}