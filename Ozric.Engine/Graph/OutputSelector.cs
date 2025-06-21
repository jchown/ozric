using System.Text.Json.Serialization;

namespace OzricEngine.Nodes;

public record OutputSelector(string nodeID, string outputName)
{
    public OutputSelector Reroute(string originalID, string newID)
    {
        return nodeID == originalID ? this with { nodeID = newID } : this;
    }

    [JsonIgnore]
    public string id => $"{nodeID}.{outputName}";
}
