using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using OzricEngine.ext;
using OzricEngine.logic;
using OzricEngine.nodes;

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

        public Dictionary<string, Node> nodes { get; set; } = new();
        public Dictionary<string, Edge> edges { get; set; } = new();
        
        public IEnumerable<string> GetNodeIDs()
        {
            return nodes.Keys;
        }

        public Node? GetNode(string nodeId)
        {
            return nodes.GetValueOrDefault(nodeId);
        }
        
        public void AddNode(Node node)
        {
            if (nodes.ContainsKey(node.id))
                throw new Exception($"A node with ID {node.id} already exists");
            
            nodes[node.id] = node;
        }

        public void RemoveNode(Node node)
        {
            if (!nodes.ContainsKey(node.id))
                throw new Exception($"No node found with ID {node.id}");

            nodes.Remove(node.id);
        }
        
        public void RemoveEdge(Edge edge)
        {
            if (!edges.ContainsKey(edge.id))
                throw new Exception($"No edge found with ID {edge.id}");

            edges.Remove(edge.id);
        }

        public void Connect(string outputNodeID, string outputPinName, string inputNodeID, string inputPinName)
        {
            if (!nodes.ContainsKey(outputNodeID))
                throw new Exception($"No node found with ID {outputNodeID}");

            if (!nodes.ContainsKey(inputNodeID))
                throw new Exception($"No node found with ID {inputNodeID}");

            var edge = new Edge(new(outputNodeID, outputPinName), new (inputNodeID, inputPinName));
            if (edges.ContainsKey(edge.id))
                throw new Exception($"An edge with ID {edge.id} already exists");
            
            edges[edge.id] = edge;

            Log(LogLevel.Debug, "{0}.{1} -> {2}.{3}", outputNodeID, outputPinName, inputNodeID, inputPinName);
        }

        public void Disconnect(string outputNodeID, string outputPinName, string inputNodeID, string inputPinName)
        {
            var edgeID = Edge.GetID(outputNodeID, outputPinName, inputNodeID, inputPinName);
            edges.Remove(edgeID);

            Log(LogLevel.Debug, "{0}.{1} xx {2}.{3}", outputNodeID, outputPinName, inputNodeID, inputPinName);
        }
        
        /// <summary>
        /// ALl the node IDs that a single node ID is connected to 
        /// </summary>

        public class NodeEdges
        {
            public readonly HashSet<string> inputNodeIDs = new();
            public readonly HashSet<string> outputNodeIDs = new();
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

            foreach (var (edgeID, edge) in edges)
            {
                var fromID = edge.from.nodeID;
                var toID = edge.to.nodeID;

                var fromNodeEdges = nodeEdges.GetOrSet(fromID, () => new NodeEdges());
                var toNodeEdges = nodeEdges.GetOrSet(toID, () => new NodeEdges());

                fromNodeEdges.outputNodeIDs.Add(toID);
                toNodeEdges.inputNodeIDs.Add(fromID);
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
            foreach (var edge in edges.Values)
            {
                dependencies.GetOrSet(edge.to.nodeID, () => new List<string>()).Add(edge.from.nodeID);
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
            foreach (var edge in edges.Values.Where(edge => edge.from.nodeID == node.id).ToList())
            {
                var value = node.GetOutputValue(edge.from.outputName);
                Log(LogLevel.Debug, "{0}.{1} = {2}", edge.to.nodeID, edge.to.inputName, value);
                nodes[edge.to.nodeID].SetInputValue(edge.to.inputName, value);
            }
        }

        #region Comparison
        public bool Equals(Graph? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return nodes.SequenceEqual(other.nodes) && edges.SequenceEqual(other.edges);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Graph)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nodes, edges);
        }
        #endregion

        public string CreateNodeID(string prefix)
        {
            int i = nodes.Count;
            while (nodes.ContainsKey($"{prefix}-{i}"))
                i++;
            return $"{prefix}-{i}";
        }

        public EntityNode? GetDevicesNode(string entityID)
        {
            return nodes.Values.Select(node => node as EntityNode).FirstOrDefault(node => node?.entityID == entityID);
        }
    }
}