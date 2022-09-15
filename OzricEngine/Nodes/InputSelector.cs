using System.Text.Json.Serialization;

namespace OzricEngine;

public record InputSelector(string nodeID, string inputName)
{
    public InputSelector Reroute(string originalID, string newID)
    {
        return nodeID == originalID ? this with { nodeID = newID } : this;
    }
    
    [JsonIgnore]
    public string id => $"{nodeID}.{inputName}";
}