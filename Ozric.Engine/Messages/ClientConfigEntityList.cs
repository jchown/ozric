namespace Ozric.Engine.Messages;

[TypeKey("config/entity_registry/list")]
public class ClientConfigEntityList : ClientCommand
{
    public ClientConfigEntityList() : base("config/entity_registry/list")
    {
    }
}