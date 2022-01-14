using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine;
using OzricEngine.ext;
using OzricEngine.logic;
using Xunit;
using ValueType = OzricEngine.logic.ValueType;

namespace OzricEngineTests
{
    public class EngineTests
    {
        [Fact]
        void nodeOrderDependsOnConnections()
        {
            var o1 = new IfAny("o1");
            var o2 = new IfAny("o2");
            var o3 = new IfAny("o3");

            var nodes = new List<Node>
            {
                new Sensor("s1", "e1"),
                new Sensor("s2", "e2"),
                new Sensor("s3", "e3"),
                new Sensor("s4", "e4"),
                o1, o2, o3
            };
            nodes.Shuffle();

            var graph = new Graph();
            foreach (var node in nodes)
                graph.AddNode(node);

            graph.Connect("s1", "activity", "o1", "i1");
            graph.Connect("s2", "activity", "o1", "i2");
            graph.Connect("s3", "activity", "o2", "i1");
            graph.Connect("s4", "activity", "o2", "i2");
            graph.Connect("o1", "output", "o3", "i1");
            graph.Connect("o2", "output", "o3", "i2");

            var ordered = graph.GetNodesInOrder();
            AssertOrder("s1", "o1", ordered);
            AssertOrder("s2", "o1", ordered);
            AssertOrder("s3", "o2", ordered);
            AssertOrder("s4", "o2", ordered);
            AssertOrder("o1", "o3", ordered);
            AssertOrder("o2", "o3", ordered);
        }

        private void AssertOrder(string before, string after, List<string> ordered)
        {
            int bi = ordered.IndexOf(before);
            int ai = ordered.IndexOf(after);
            Assert.False(bi == -1 || ai == -1);
            Assert.True(bi < ai);
        }

        [Fact]
        async Task canProcessSunEvents()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            var phases = new DayPhases("phase_id");
            phases.AddOutput("color", ValueType.Color);
            phases.AddPhase(DayPhases.PhaseStart.Create(DayPhases.SunPhase.Midnight, 0, new Mode("am")));
            phases.AddPhase(DayPhases.PhaseStart.Create(DayPhases.SunPhase.Noon, 0, new Mode("pm")));

            await phases.OnInit(context);
            engine.ProcessMockEvent("sun_event");
            await phases.OnUpdate(context);
        }

        [Fact]
        void canProcessHacsEvents()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);

            engine.ProcessMockEvent("hacs_repository");

            engine.ProcessMockEvent("hacs_config");
        }

        [Fact]
        void canProcessZHAEvents()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);

            engine.ProcessMockEvent("zha_event");
        }

        [Fact]
        void canProcessLightEvents()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);

            engine.ProcessMockEvent("light_event");
        }

        [Fact]
        async Task sensorEventWillTriggerStateChange()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sensor_1");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            var sensor = new Sensor("sensor_1", "binary_sensor.sensor_1");
            engine.graph.AddNode(sensor);

            await sensor.OnInit(context);
            Assert.Equal(new OnOff(false), sensor.GetOutputOnOff("activity"));

            engine.ProcessMockEvent("sensor_1_on");
            await sensor.OnUpdate(context);
            Assert.Equal(new OnOff(true), sensor.GetOutputOnOff("activity"));
        }

        class VerboseObject : OzricObject
        {
            public override string Name => "Logger";

            public void ExampleLog(LogLevel level)
            {
                Log(level, "Log level {0}", level);
            }
        }

        [Fact]
        void canSendLogsOfAllLevel()
        {
            var vo = new VerboseObject();

            foreach (var level in Enum.GetValues(typeof(LogLevel)))
            {
                vo.ExampleLog((LogLevel)level);
            }
        }
    }
}