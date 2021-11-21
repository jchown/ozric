namespace OzricEngine
{
    [TypeKey("auth_ok")]
    public class ServerAuthOK: ServerMessage
    {
        public string ha_version { get; set; }
    }
}