using OzricEngine.Nodes;

namespace OzricUI;

public class Zone : IGraphObject
{
    public string id { get; }
    public List<String> nodeIDs;

    public Zone(string id, List<string> nodeIDs)
    {
        this.id = id;
        this.nodeIDs = nodeIDs;
    }
}