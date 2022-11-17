using OzricEngine.Nodes;

namespace OzricUI;

public class Zone : IGraphObject
{
    public string id { get; }
    
    public List<String> nodeIDs = new();

    public Zone(string id)
    {
        this.id = id;
    }
}