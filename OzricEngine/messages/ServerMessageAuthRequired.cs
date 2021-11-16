namespace OzricEngine
{
    [TypeKey("auth_required")]
    public class ServerMessageAuthRequired: ServerMessage
    {
        public string ha_version { get; set; }
    }
}