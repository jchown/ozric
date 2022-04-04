using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    /// <summary>
    /// The nodes & edges (connections between nodes) that make up the logical flow of our system.
    /// More strictly a Directed Acyclic Graph (DAG).  
    /// </summary>
    public class Graph: OzricObject, IEquatable<Graph>
    {
        [JsonIgnore]
        public override string Name => "Graph";

        public Dictionary<string, Node> nodes { get; set; }
        public Dictionary<string, Dictionary<string, List<InputSelector>>> edges { get; set; }

        public Graph()
        {
            nodes = new Dictionary<string, Node>();
            edges = new Dictionary<string, Dictionary<string, List<InputSelector>>>();
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
            if (nodes.ContainsKey(node.id))
                throw new Exception($"A node with ID {node.id} already exists");
            
            nodes[node.id] = node;
        }

        public void Connect(string outputNodeID, string outputPinName, string inputNodeID, string inputPinName)
        {
            if (!nodes.ContainsKey(outputNodeID))
                throw new Exception($"No node found with ID {outputNodeID}");

            if (!nodes.ContainsKey(inputNodeID))
                throw new Exception($"No node found with ID {inputNodeID}");

            var nodeOutputs = edges.GetOrSet(outputNodeID, () => new Dictionary<string, List<InputSelector>>());
            var pinOutputs = nodeOutputs.GetOrSet(outputPinName, () => new List<InputSelector>()); 
            pinOutputs.Add(new InputSelector(inputNodeID, inputPinName));

            Log(LogLevel.Debug, "{0}.{1} -> {2}.{3}", outputNodeID, outputPinName, inputNodeID, inputPinName);
        }

        public void Disconnect(string outputNodeID, string outputPinName, string inputNodeID, string inputPinName)
        {
            edges[outputNodeID][outputPinName].Remove(new InputSelector(inputNodeID, inputPinName));

            Log(LogLevel.Debug, "{0}.{1} xx {2}.{3}", outputNodeID, outputPinName, inputNodeID, inputPinName);
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

            foreach (var (fromID, outputs) in edges)
            {
                var fromNodeEdges = nodeEdges.GetOrSet(fromID, () => new NodeEdges());

                foreach (var inputs in outputs.Values)
                {
                    foreach (var input in inputs)
                    {
                        var toID = input.nodeID;

                        var toNodeEdges = nodeEdges.GetOrSet(toID, () => new NodeEdges());

                        fromNodeEdges.outputNodeIDs.Add(toID);
                        toNodeEdges.inputNodeIDs.Add(fromID);
                    }
                }
            }

            return nodeEdges;
        }
                
        /// <summary>
        /// All the Home entity IDs referenced by this graph.
        /// </summary>
        /// <returns></returns>

        public List<string> GetInterestedEntityIDs()
        {
            return nodes.Values.Select(node => node as EntityNode).Where(node => node != null).Select(node => node.entityID).Distinct().ToList();
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
            foreach (var (outputNodeID, outputs) in edges)
            {
                foreach (var inputs in outputs.Values)
                {
                    foreach (var input in inputs)
                    {
                        dependencies.GetOrSet(input.nodeID, () => new List<string>()).Add(outputNodeID);
                    }
                }
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
                var value = output.value;

                var nodeOutputs = edges.GetValueOrDefault(node.id);
                if (nodeOutputs == null)
                {
                    Log(LogLevel.Warning, "Missing outputs for node {0}", node.id);
                    continue;
                }

                var outputs = nodeOutputs.GetValueOrDefault(output.name);
                if (outputs == null)
                {
                    Log(LogLevel.Warning, "Missing outputs for {0}.{1}", node.id, output.name);
                    continue;
                }

                foreach (var input in outputs)
                {
                    Log(LogLevel.Debug, "{0}.{1} = {2}", input.nodeID, input.inputName, value);
                    nodes[input.nodeID].SetInputValue(input.inputName, value);
                }
            }
        }

        public bool Equals(Graph other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(nodes, other.nodes) && Equals(edges, other.edges);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Graph)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nodes, edges);
        }
    }
}