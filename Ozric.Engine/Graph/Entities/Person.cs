namespace OzricEngine.Nodes;

[TypeKey(NodeType.Person)]
public class Person : ModeSensor
{
    public override NodeType nodeType => NodeType.Person;

    public Person(string id, string entityID) : base(id, entityID)
    {
    }
}