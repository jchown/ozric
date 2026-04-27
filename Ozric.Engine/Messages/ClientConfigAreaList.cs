namespace Ozric.Engine.Messages;

[TypeKey("config/area_registry/list")]
public class ClientConfigAreaList : ClientCommand
{
    public ClientConfigAreaList() : base("config/area_registry/list")
    {
    }
}