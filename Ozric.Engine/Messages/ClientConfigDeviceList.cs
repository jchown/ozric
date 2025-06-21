namespace OzricEngine.messages;

[TypeKey("config/device_registry/list")]
public class ClientConfigDeviceList : ClientCommand
{
    public ClientConfigDeviceList() : base("config/device_registry/list")
    {
    }
}