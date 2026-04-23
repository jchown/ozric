using System;
using System.Collections.Generic;
using Ozric.Engine.Graph;
using OzricEngine.Values;
using OzricEngineTests;
using Xunit;
using ValueType = Ozric.Engine.Values.ValueType;

namespace OzricEngine.Nodes
{
    public class DayPhasesTests
    {
        [Fact]
        public void canSplitDayInToMorningAndEvening()
        {
            var node = new GraphDayPhases("am_pm");
            node.outputs.Add(new Pin("morning", ValueType.Binary));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("am"), GraphDayPhases.SunPhase.Midnight));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("pm"), GraphDayPhases.SunPhase.Noon));

            var homePM = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_morning");
            var enginePM = new MockEngine(homePM);
            node.OnInit(new MockContext(enginePM));
            Assert.Equal(new Mode("pm"), node.GetOutput("mode").value);

            var homeAM = new MockHome(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"), "sun_morning");
            var engineAM = new MockEngine(homeAM);
            node.OnInit(new MockContext(engineAM));
            Assert.Equal(new Mode("am"), node.GetOutput("mode").value);
        }

        [Fact]
        public void canEvaluatePhaseCorrectly()
        {
            var node = new GraphDayPhases("Color-phases");
            node.AddOutput("Color", ValueType.Color);
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("after-midnight"), GraphDayPhases.SunPhase.Midnight));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("after-6am"), GraphDayPhases.SunPhase.Midnight, + 6 * 60 * 60));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("after-noon"), GraphDayPhases.SunPhase.Noon));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("after-6pm"), GraphDayPhases.SunPhase.Midnight, - 6 * 60 * 60));
            
            var home = new MockHome(DateTime.Parse("2021-11-29T03:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);
            
            node.OnInit(context);

            Assert.Equal(new Mode("after-midnight"), node.GetOutput("mode").value);

            home.SetTime(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-6am"), node.GetOutput("mode").value);

            home.SetTime(DateTime.Parse("2021-11-29T13:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-noon"), node.GetOutput("mode").value);

            home.SetTime(DateTime.Parse("2021-11-29T23:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-6pm"), node.GetOutput("mode").value);
        }

        [Fact]
        public void notConfusedByPreMidnightPhases()
        {
            var node = new GraphDayPhases("kitchen-lights-phases");
            node.AddOutput("colour", ValueType.Color);

            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("near-midnight"), GraphDayPhases.SunPhase.Midnight, - 30 * 60));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("after-6am"), GraphDayPhases.SunPhase.Midnight, + 6 * 60 * 60));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("sun-setting"), GraphDayPhases.SunPhase.Setting, - 30 * 60));
            node.AddPhase(new GraphDayPhases.PhaseStart(new Mode("after-7pm"), GraphDayPhases.SunPhase.Midnight, - 5 * 60 * 60));

            var home = new MockHome(DateTime.Parse("2021-11-29T03:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            node.OnInit(context);
            Assert.Equal(new Mode("near-midnight"), node.GetOutput("mode").value);

            home.SetTime(DateTime.Parse("2021-11-29T09:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-6am"), node.GetOutput("mode").value);

            home.SetTime(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("after-7pm"), node.GetOutput("mode").value);

            home.SetTime(DateTime.Parse("2021-11-29T23:41:25.459551+00:00"));
            node.OnUpdate(context);
            Assert.Equal(new Mode("near-midnight"), node.GetOutput("mode").value);
        }
    }
}