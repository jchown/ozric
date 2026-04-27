using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ozric.Engine.Graph.Logic;
using Ozric.Engine.Nodes;
using Ozric.Engine.Values;
using Xunit;

namespace Ozric.Engine.Tests
{
    using Graph = Ozric.Engine.Graph.Graph;

    public class GraphTests
    {
        [Fact]
        public void CanGetNodeOrdering()
        {
            var graph = GetSimpleGraph();
            
            Assert.Equal(new List<string>{"a","b","c"}, graph.GetNodesInOrder());
        }

        [Fact]
        public void CanGetNodeDependencies()
        {
            var graph = GetSimpleGraph();

            var nodeEdgesMap = graph.GetNodeDependencies();
            
            AssertDependencies(nodeEdgesMap["a"].inputNodeIDs);
            AssertDependencies(nodeEdgesMap["a"].outputNodeIDs, "b", "c");
            
            AssertDependencies(nodeEdgesMap["b"].inputNodeIDs, "a");
            AssertDependencies(nodeEdgesMap["b"].outputNodeIDs, "c");
            
            AssertDependencies(nodeEdgesMap["c"].inputNodeIDs, "a", "b");
            AssertDependencies(nodeEdgesMap["c"].outputNodeIDs);
        }

        [Fact]
        public void CanSerialiseRoundTrip()
        {
            var a = GetSimpleGraph();

            Assert.Equal(3, a.nodes.Count);
            Assert.Single(a.nodes["b"].inputs);

            var json = Json.Serialize(a);
            var b = Graph.Deserialize(json);
            
            Assert.Equal(3, b.nodes.Count);
            Assert.Single(b.nodes["b"].inputs);
        }

        [Fact]
        public void CanDeserialiseLargeGraph()
        {
            var json = File.ReadAllText("../../../graphs/chown_graph_1.json");
            var graph = Graph.Deserialize(json);
            
            Assert.Equal("kitchen-sensor", graph.nodes["kitchen-sensor"].id);
        }
        
        private void AssertDependencies(HashSet<string> nodeIDs, params string[] expected)
        {
            Assert.Equal(expected.ToList(), nodeIDs.ToList().OrderBy(s => s));
        }

        private static Graph GetSimpleGraph()
        {
            var graph = new Graph();
            var a = new GraphConstant("a", new Binary(false));
            var b = new GraphIfAny("b");
            var c = new GraphIfAny("c");

            b.AddInput("value", ValueType.Binary);
            c.AddInput("value1", ValueType.Binary);
            c.AddInput("value2", ValueType.Binary);

            graph.AddNode(c);
            graph.AddNode(a);
            graph.AddNode(b);

            graph.Connect("a", "value", "b", "value");
            graph.Connect("a", "value", "c", "value1");
            graph.Connect("b", "value", "c", "value2");
            return graph;
        }
    }
}
