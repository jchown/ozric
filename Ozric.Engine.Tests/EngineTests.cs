using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozric.Engine.Extensions;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Graph.Logic;
using Ozric.Engine.Nodes;
using Ozric.Engine.Utils;
using OzricEngine;
using OzricEngine.Nodes;
using Xunit;
using OzricEngine.Values;
using ValueType = Ozric.Engine.Values.ValueType;

namespace OzricEngineTests
{
    public class EngineTests
    {
        [Fact]
        void nodeOrderDependsOnConnections()
        {
            var o1 = new GraphIfAny("o1");
            var o2 = new GraphIfAny("o2");
            var o3 = new GraphIfAny("o3");

            var nodes = new List<GraphNode>
            {
                new GraphBinarySensor("s1", "e1"),
                new GraphBinarySensor("s2", "e2"),
                new GraphBinarySensor("s3", "e3"),
                new GraphBinarySensor("s4", "e4"),
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

            var phases = new GraphDayPhases("phase_id");
            phases.AddOutput("color", ValueType.Color);
            phases.AddPhase(GraphDayPhases.PhaseStart.Create(GraphDayPhases.SunPhase.Midnight, 0, new Mode("am")));
            phases.AddPhase(GraphDayPhases.PhaseStart.Create(GraphDayPhases.SunPhase.Noon, 0, new Mode("pm")));

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
            engine.ProcessMockEvent("hacs_stage");
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

            engine.ProcessMockEvent("light_on");
        }

        [Fact]
        void canProcessEntityRegistryUpdatedEvents()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_evening", "weather_sunny");
            var engine = new MockEngine(home);

            engine.ProcessMockEvent("entity_registry_updated");
        }

        [Fact]
        async Task sensorEventWillTriggerStateChange()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sensor_1");
            var engine = new MockEngine(home);
            var context = new MockContext(engine);

            var sensor = new GraphBinarySensor("sensor_1", "binary_sensor.sensor_1");
            engine.graph.AddNode(sensor);

            await sensor.OnInit(context);
            Assert.Equal(new Binary(false), sensor.GetOutputValue<Binary>("activity"));

            engine.ProcessMockEvent("sensor_1_on");
            await sensor.OnUpdate(context);
            Assert.Equal(new Binary(true), sensor.GetOutputValue<Binary>("activity"));
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

        /*
        [Fact]
        async Task serviceCallsReconciled()
        {
            var home = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sensor_1");
            var engine = new MockEngine(home);

            var sender = new MockCommandBatcher("lights_on");
            await engine.SendCommands();

            engine.ProcessMockEvent("lights_on");
            Assert.False(engine.ProcessMockEvent("light_on"));
        }
        */
    }
}