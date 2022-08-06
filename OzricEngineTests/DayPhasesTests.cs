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
            node.outputs.Add(new Pin("morning", ValueType.Boolean));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("am"), DayPhases.SunPhase.Midnight));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("pm"), DayPhases.SunPhase.Noon));

            var homePM = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_morning");
            var enginePM = new MockEngine(homePM);
            node.OnInit(new MockContext(enginePM));
            Assert.Equal(new Mode("pm"), node.GetOutputValue("mode"));

            var homeAM = new MockHome(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"), "sun_morning");
            var engineAM = new MockEngine(homeAM);
            node.OnInit(new MockContext(engineAM));
            Assert.Equal(new Mode("am"), node.GetOutputValue("mode"));
        }

        [Fact]
        public void canEvaluatePhaseCorrectly()
        {
            var node = new DayPhases("Color-phases");
            node.AddOutput("Color", ValueType.Color);
            node.AddPhase(new DayPhases.PhaseStart(new Mode("after-midnight"), DayPhases.SunPhase.Midnight));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("after-6am"), DayPhases.SunPhase.Midnight, + 6 * 60 * 60));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("after-noon"), DayPhases.SunPhase.Noon));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("after-6pm"), DayPhases.SunPhase.Midnight, - 6 * 60 * 60));
            
            var home = new MockHome(DateTime.Parse("2021-11-29T03:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);
            
            node.OnInit(context);

            Assert.Equal(new Mode("after-midnight"), node.GetOutputValue("mode"));

            home.SetTime(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-6am"), node.GetOutputValue("mode"));

            home.SetTime(DateTime.Parse("2021-11-29T13:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-noon"), node.GetOutputValue("mode"));

            home.SetTime(DateTime.Parse("2021-11-29T23:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-6pm"), node.GetOutputValue("mode"));
        }

        [Fact]
        public void notConfusedByPreMidnightPhases()
        {
            var node = new DayPhases("kitchen-lights-phases");
            node.AddOutput("colour", ValueType.Color);

            node.AddPhase(new DayPhases.PhaseStart(new Mode("near-midnight"), DayPhases.SunPhase.Midnight, - 30 * 60));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("after-6am"), DayPhases.SunPhase.Midnight, + 6 * 60 * 60));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("sun-setting"), DayPhases.SunPhase.Setting, - 30 * 60));
            node.AddPhase(new DayPhases.PhaseStart(new Mode("after-7pm"), DayPhases.SunPhase.Midnight, - 5 * 60 * 60));

            var home = new MockHome(DateTime.Parse("2021-11-29T03:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            node.OnInit(context);
            Assert.Equal(new Mode("near-midnight"), node.GetOutputValue("mode"));

            home.SetTime(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-6am"), node.GetOutputValue("mode"));

            home.SetTime(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-7pm"), node.GetOutputValue("mode"));

            home.SetTime(DateTime.Parse("2021-11-29T23:41:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("near-midnight"), node.GetOutputValue("mode"));
        }
    }
}