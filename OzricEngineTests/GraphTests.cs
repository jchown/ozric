using OzricEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OzricEngine.logic;
using Xunit;

namespace OzricEngineTests
{
    public class GraphTests
    {
        [Fact]
        public void canGetNodeOrdering()
        {
            var graph = GetSimpleGraph();
            
            Assert.Equal(new List<string>{"a","b","c"}, graph.GetNodesInOrder());
        }

        [Fact]
        public void canGetNodeDependencies()
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
        public void canSerialiseRoundTrip()
        {
            var a = GetSimpleGraph();
            var json = Json.Serialize(a);
            var b = Json.Deserialize<Graph>(json);
            
            Assert.Equal(3, b.nodes.Count);
            Assert.Single(b.nodes["b"].inputs);
        }

        private void AssertDependencies(HashSet<string> nodeIDs, params string[] expected)
        {
            Assert.Equal(expected.ToList(), nodeIDs.ToList().OrderBy(s => s));
        }

        private static Graph GetSimpleGraph()
        {
            var graph = new Graph();
            var a = new Constant("a", new OnOff());
            var b = new IfAny("b");
            var c = new IfAny("c");

            b.AddInput("value", ValueType.OnOff);
            c.AddInput("value1", ValueType.OnOff);
            c.AddInput("value2", ValueType.OnOff);

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
