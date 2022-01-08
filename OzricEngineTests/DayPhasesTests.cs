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
            node.outputs.Add(new Pin("morning", ValueType.OnOff));
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
            node.AddOutput("Color", ValueType.Color);
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(1,0,0,0) } }, DayPhases.SunPhase.Midnight));
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(0,1,0,0) } }, DayPhases.SunPhase.Midnight, + 6 * 60 * 60));
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(0,0,1,0) } }, DayPhases.SunPhase.Noon));
            node.AddPhase(new DayPhases.PhaseStart(new Dictionary<string, Value> { { "Color", new ColorRGB(0,0,0,1) } }, DayPhases.SunPhase.Midnight, - 6 * 60 * 60));
            
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

        [Fact]
        public void notConfusedByPreMidnightPhases()
        {
            var node = new DayPhases("kitchen-lights-phases");
            node.AddOutput("colour", ValueType.Color);

            var afterMidnight = DayPhases.PhaseStart.Create(DayPhases.SunPhase.Midnight, - 34 * 60, ("colour", ColorRGB.RED));
            var after6am = DayPhases.PhaseStart.Create(DayPhases.SunPhase.Midnight, 6 * 60 * 60, ("colour", ColorRGB.GREEN));
            var afterSunSetting = DayPhases.PhaseStart.Create(DayPhases.SunPhase.Setting, -30 * 60, ("colour", ColorRGB.BLUE));
            var after8pm = DayPhases.PhaseStart.Create(DayPhases.SunPhase.Midnight, -8 * 60 * 60, ("colour", ColorRGB.WHITE));
        
            node.phases.Add(afterMidnight);
            node.phases.Add(after6am);
            node.phases.Add(afterSunSetting);
            node.phases.Add(after8pm);

            var home = new MockHome(DateTime.Parse("2021-11-29T03:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(home);

            node.OnInit(engine);

            home.SetTime(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"));
            node.OnUpdate(engine);
            Assert.Equal(ColorRGB.GREEN, node.GetOutputColor("colour"));

            home.SetTime(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"));
            node.OnUpdate(engine);
            Assert.Equal(ColorRGB.BLUE, node.GetOutputColor("colour"));

            home.SetTime(DateTime.Parse("2021-11-29T23:41:25.459551+00:00"));
            node.OnUpdate(engine);
            Assert.Equal(ColorRGB.RED, node.GetOutputColor("colour"));
        }
    }
}