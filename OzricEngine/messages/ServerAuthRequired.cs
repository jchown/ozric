namespace OzricEngine
{
    [TypeKey("auth_required")]
    public class ServerAuthRequired: ServerMessage
    {
        public string ha_version;
    }
}