namespace OzricEngine
{
    [ServerResultType(Type.auth_ok)]
    public class ServerAuthOK: ServerResult
    {
        public string ha_version;
    }
    
    [ServerResultType(Type.auth_invalid)]
    public class ServerAuthInvalid: ServerResult
    {
        public string message;
    }

}