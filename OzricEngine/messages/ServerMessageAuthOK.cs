namespace OzricEngine
{
    [TypeKey("auth_ok")]
    public class ServerMessageAuthOK: ServerMessage
    {
        public string ha_version { get; set; }
    }
}