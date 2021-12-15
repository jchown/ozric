using System.Collections.Generic;
using OzricEngine;
using OzricEngine.ext;
using OzricEngine.logic;
using Xunit;

namespace OzricEngineTests
{
    public class EngineTests
    {
        [Fact]
        void nodeOrderDependsOnConnections()
        {
            var o1 = new Or("o1");
            var o2 = new Or("o2");
            var o3 = new Or("o3");

            var nodes = new List<Node>
            {
                new Sensor("s1", "e1"),
                new Sensor("s2", "e2"),
                new Sensor("s3", "e3"),
                new Sensor("s4", "e4"),
                o1, o2, o3
            };
            nodes.Shuffle();

            var engine = new MockEngine();
            foreach (var node in nodes)
                engine.Add(node);
            
            engine.Connect(new OutputSelector { nodeID = "s1", outputName = "activity"}, new InputSelector { nodeID = "o1", inputName = "i1" });
            engine.Connect(new OutputSelector { nodeID = "s2", outputName = "activity"}, new InputSelector { nodeID = "o1", inputName = "i2" });
            engine.Connect(new OutputSelector { nodeID = "s3", outputName = "activity"}, new InputSelector { nodeID = "o2", inputName = "i1" });
            engine.Connect(new OutputSelector { nodeID = "s4", outputName = "activity"}, new InputSelector { nodeID = "o2", inputName = "i2" });
            engine.Connect(new OutputSelector { nodeID = "o1", outputName = "output"}, new InputSelector { nodeID = "o3", inputName = "i1" });
            engine.Connect(new OutputSelector { nodeID = "o2", outputName = "output"}, new InputSelector { nodeID = "o3", inputName = "i2" });

            var ordered = engine.GetNodesInOrder();
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
    }
}