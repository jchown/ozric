using System.Text.Json.Serialization;
using OzricEngine.logic;

namespace OzricEngine.nodes;

public record Edge(OutputSelector from, InputSelector to) : IGraphObject
{
    [JsonIgnore]
    public string id => GetID(from.nodeID, from.outputName, to.nodeID, to.inputName);

    public static string GetID(string fromNodeID, string fromOutputName, string toNodeID, string toInputName)
    {
        return $"{fromNodeID}/{fromOutputName}:{toNodeID}/{toInputName}";
    }

    public Edge Reroute(string originalID, string newID)
    {
        return new Edge(from.Reroute(originalID, newID), to.Reroute(originalID, newID));
    }
}
