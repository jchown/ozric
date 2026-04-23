using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Ozric.Engine.Extensions;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Utils;
using OzricEngine;
using OzricEngine.Nodes;

namespace Ozric.Engine.Graph;

/// <summary>
/// The nodes &amp; edges (connections between nodes) that make up the logical flow of our system.
/// More strictly a Directed Acyclic Graph (DAG).  
/// </summary>
public class Graph: OzricObject, IEquatable<Graph>, IGraph
{
    [JsonIgnore]
    public override string Name => "Graph";

    public Dictionary<string, GraphNode> nodes { get; set; } = new();
    public Dictionary<string, GraphEdge> edges { get; set; } = new();
        
    public IEnumerable<string> GetNodeIDs()
    {
        return nodes.Keys;
    }

    public GraphNode? GetNode(string nodeId)
    {
        return nodes.GetValueOrDefault(nodeId);
    }
        
    public void AddNode(GraphNode graphNode)
    {
        if (!nodes.TryAdd(graphNode.id, graphNode))
            throw new Exception($"A node with ID {graphNode.id} already exists");
    }

    public void RemoveNode(GraphNode graphNode)
    {
        if (!nodes.Remove(graphNode.id))
            throw new Exception($"No node found with ID {graphNode.id}");
    }
        
    public void RemoveEdge(GraphEdge graphEdge)
    {
        if (!edges.Remove(graphEdge.id))
            throw new Exception($"No edge found with ID {graphEdge.id}");
    }

    public void Connect(string outputNodeID, string outputPinName, string inputNodeID, string inputPinName)
    {
        if (!nodes.ContainsKey(outputNodeID))
            throw new Exception($"No node found with ID {outputNodeID}");

        if (!nodes.ContainsKey(inputNodeID))
            throw new Exception($"No node found with ID {inputNodeID}");

        var edge = new GraphEdge(new(outputNodeID, outputPinName), new (inputNodeID, inputPinName));
        if (!edges.TryAdd(edge.id, edge))
            throw new Exception($"An edge with ID {edge.id} already exists");

        Log(LogLevel.Debug, "{0}.{1} -> {2}.{3}", outputNodeID, outputPinName, inputNodeID, inputPinName);
    }

    public void Disconnect(string outputNodeID, string outputPinName, string inputNodeID, string inputPinName)
    {
        var edgeID = GraphEdge.GetID(outputNodeID, outputPinName, inputNodeID, inputPinName);
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
        return nodes.Values.Select(node => node as GraphEntity).Where(node => node != null).Select(node => node!.entityID).Distinct().ToList();
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
            foreach (var nodeID in dependency.Value.ToArray())
            {
                if (!dependencies.ContainsKey(nodeID))
                {
                    Log(LogLevel.Warning, $"Node '{dependency.Key}' depends on '{nodeID}', but is not in graph");
                    dependency.Value.Remove(nodeID);
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
    /// <param name="graphNode"></param>
    /// <param name="context"></param>
    public void CopyNodeOutputValues(GraphNode graphNode, Context context)
    {
        foreach (var edge in edges.Values.Where(edge => edge.from.nodeID == graphNode.id).ToList())
        {
            var value = graphNode.GetOutput(edge.from.outputName).value;
            if (value == null)
                continue;
                
            Log(LogLevel.Debug, "{0}.{1} = {2}", edge.to.nodeID, edge.to.inputName, value);
            nodes[edge.to.nodeID].SetInputValue(edge.to.inputName, value, context);
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

    public IList<GraphNode> GetConnectedNodes(string nodeId)
    {
        if (!nodes.ContainsKey(nodeId))
            throw new Exception($"No node found with ID {nodeId}");

        return edges.Values
            .Where(edge => edge.from.nodeID == nodeId || edge.to.nodeID == nodeId)
            .Select(edge => nodes[edge.from.nodeID == nodeId ? edge.to.nodeID : edge.from.nodeID])
            .Distinct()
            .ToList();
    }

    #endregion

    public string CreateNodeID(string prefix)
    {
        int i = nodes.Count;
        while (nodes.ContainsKey($"{prefix}-{i}"))
            i++;
        return $"{prefix}-{i}";
    }

    public bool HasEntityNode(string entityID)
    {
        return nodes.Values.Any(node => node is GraphEntity en && en.entityID == entityID);
    }

    public GraphEntity GetEntityNode(string entityID)
    {
        return (GraphEntity) nodes.Values.First(node => node is GraphEntity en && en.entityID == entityID);
    }

    public List<T> GetAll<T>() where T : class
    {
        return nodes.Values.Select(n => n as T).Where(n => n != null).Select(n => n!).ToList();
    }

    public static Graph Deserialize(string json)
    {
        //  TODO: Have schema version

        json = json.Replace("OnOff", "binary");
        json = json.Replace("boolean", "binary");
        json = json.Replace("Boolean", "Binary");
        json = json.Replace("scalar", "number");
        json = json.Replace("Scalar", "Number");

        return Json.Deserialize<Graph>(json);
    }

    public bool HasOutput(OutputSelector output)
    {
        if (!nodes.ContainsKey(output.nodeID))
            return false;
            
        if (!nodes[output.nodeID].HasOutput(output.outputName))
            return false;

        return true;
    }

    public bool HasInput(InputSelector output)
    {
        if (!nodes.ContainsKey(output.nodeID))
            return false;
            
        if (!nodes[output.nodeID].HasInput(output.inputName))
            return false;

        return true;
    }

    public bool HasNode(string nodeId)
    {
        return nodes.ContainsKey(nodeId);
    }

    public IEnumerable<GraphNode> GetOutputs(GraphNode graphNode)
    {
        return edges.Values.Where(edge => edge.from.nodeID == graphNode.id).Select(edge => nodes[edge.to.nodeID]);
    }
}