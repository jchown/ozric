namespace OzricEngine
{
    [ServerResultType("auth_ok")]
    public class ServerAuthOK: ServerResult
    {
        public string ha_version;
    }
}