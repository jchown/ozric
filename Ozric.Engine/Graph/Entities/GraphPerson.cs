namespace OzricEngine.Nodes;

[TypeKey(NodeType.Person)]
public class GraphPerson : GraphModeSensor
{
    public override NodeType nodeType => NodeType.Person;

    public GraphPerson(string id, string entityID) : base(id, entityID)
    {
    }
}