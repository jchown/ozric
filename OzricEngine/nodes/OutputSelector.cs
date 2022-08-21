namespace OzricEngine;

public record OutputSelector(string nodeID, string outputName)
{
    public OutputSelector Reroute(string originalID, string newID)
    {
        return nodeID == originalID ? this with { nodeID = newID } : this;
    }
}
