namespace OzricEngine
{
    [ServerResultType("auth_invalid")]
    public class ServerAuthInvalid: ServerResult
    {
        public string message;
    }

}