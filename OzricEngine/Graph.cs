using System;
using System.Collections.Generic;
using System.Linq;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    /// <summary>
    /// The nodes & edges (connections between nodes) that make up the logical flow of our system.
    /// More stricly a Directed Acyclic Graph (DAG).  
    /// </summary>
    public class Graph: OzricObject
    {
        public override string Name => "Graph";

        private Dictionary<string, Node> nodes { get; }
        private Dictionary<OutputSelector, List<InputSelector>> edges { get; }

        public Graph()
        {
            nodes = new Dictionary<string, Node>();
            edges = new Dictionary<OutputSelector, List<InputSelector>>();
        }
        
        public IEnumerable<string> GetNodeIDs()
        {
            return nodes.Keys;
        }

        public Node GetNode(string nodeId)
        {
            return nodes.GetValueOrDefault(nodeId);
        }
        
        public void AddNode(Node node)
        {
            nodes[node.id] = node;
        }

        public void Connect(OutputSelector output, InputSelector input)
        {
            if (!nodes.ContainsKey(output.nodeID))
                throw new Exception();

            if (!nodes.ContainsKey(input.nodeID))
                throw new Exception();

            edges.GetOrSet(output, () => new List<InputSelector>()).Add(input);

            Log(LogLevel.Debug, "{0}.{1} -> {2}.{3}", output.nodeID, output.outputName, input.nodeID, input.inputName);
        }

        public void Disconnect(OutputSelector output, InputSelector input)
        {
            edges.Get(output).Remove(input);
        }
        
        /// <summary>
        /// ALl the node IDs that a single node ID is connected to 
        /// </summary>

        public class NodeEdges
        {
            public readonly HashSet<string> inputNodeIDs = new HashSet<string>();
            public readonly HashSet<string> outputNodeIDs = new HashSet<string>();
        }

        /// <summary>
        /// Return a mapping of nodes -> list of nodes that are dependencies of, and dependent on, that node.
        /// e.g. For a graph of [A -> B, A -> C] return [A -> [][B,C], B -> [A][], C -> [A][] ]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, NodeEdges> GetNodeDependencies()
        {
            //  Work out the dependencies for each node

            var nodeEdges = new Dictionary<string, NodeEdges>();

            foreach (var (outputSelector, inputSelectors) in edges)
            {
                var fromID = outputSelector.nodeID;
                var fromNodeEdges = nodeEdges.GetOrSet(fromID, () => new NodeEdges());

                foreach (var output in inputSelectors)
                {
                    var toID = output.nodeID;

                    var toNodeEdges = nodeEdges.GetOrSet(toID, () => new NodeEdges());

                    fromNodeEdges.outputNodeIDs.Add(toID);
                    toNodeEdges.inputNodeIDs.Add(fromID);
                }
            }

            return nodeEdges;
        }
        
        /// <summary>
        /// Get the nodes in update order, such that all inputs
        /// can be read after being written by their upstream outputs.
        /// </summary>
        /// <returns></returns>
        public List<string> GetNodesInOrder()
        {
            //  Work out the dependencies for each node

            var dependencies = new Dictionary<string, List<string>>();
            foreach (var edge in edges)
            {
                var output = edge.Key.nodeID;

                foreach (var input in edge.Value)
                    dependencies.GetOrSet(input.nodeID, () => new List<string>()).Add(output);
            }

            //  Some nodes have no edges

            foreach (var node in nodes)
            {
                if (!dependencies.ContainsKey(node.Key))
                    dependencies[node.Key] = new List<string>();
            }

            //  Check no dependencies are missing

            foreach (var dependency in dependencies)
            {
                foreach (var nodeID in dependency.Value)
                {
                    if (!dependencies.ContainsKey(nodeID))
                    {
                        throw new Exception($"Node '{dependency.Key}' depends on '{nodeID}', but is not in graph");
                    }
                }
            }

            //  Now can walk through picking nodes that either have no dependencies
            //  or all its dependencies have already been picked 

            var unordered = new List<string>(nodes.Keys);
            var ordered = new List<string>();

            while (unordered.Count > 0)
            {
                var nextID = unordered.FirstOrDefault(nodeID => { return dependencies.Get(nodeID)?.All(input => ordered.Contains(input)) ?? true; });

                if (nextID == null)
                    throw new Exception($"Cannot order nodes, cycle in graph?\nOrdered = {ordered.Join(",")}\nUnordered = {unordered.Join(",")}");

                ordered.Add(nextID);
                unordered.Remove(nextID);
            }

            return ordered;
        }
        
        /// <summary>
        /// Copy all output values to connected nodes' inputs
        /// </summary>
        /// <param name="node"></param>
        public void CopyNodeOutputValues(Node node)
        {
            foreach (var output in node.outputs)
            {
                var selector = new OutputSelector { nodeID = node.id, outputName = output.name };
                var value = output.value;

                if (!edges.ContainsKey(selector))
                {
                    Log(LogLevel.Warning, "Missing output selector for {0}.{1}", node.id, output.name);
                    continue;
                }

                foreach (var input in edges[selector])
                {
                    Log(LogLevel.Debug, "{0}.{1} = {2}", input.nodeID, input.inputName, value);

                    nodes[input.nodeID].SetInputValue(input.inputName, value);
                }
            }
        }
    }
}