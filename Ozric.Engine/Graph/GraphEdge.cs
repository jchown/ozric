using System.Text.Json.Serialization;

namespace Ozric.Engine.Graph;

public record GraphEdge(OutputSelector from, InputSelector to) : IGraphObject
{
    [JsonIgnore]
    public string id => GetID(from.nodeID, from.outputName, to.nodeID, to.inputName);

    public static string GetID(string fromNodeID, string fromOutputName, string toNodeID, string toInputName)
    {
        return $"{fromNodeID}/{fromOutputName}:{toNodeID}/{toInputName}";
    }

    public GraphEdge Reroute(string originalID, string newID)
    {
        return new GraphEdge(from.Reroute(originalID, newID), to.Reroute(originalID, newID));
    }
}
