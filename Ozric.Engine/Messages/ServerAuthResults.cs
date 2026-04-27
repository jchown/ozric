namespace Ozric.Engine
{
    [TypeKey("auth_invalid")]
    public class ServerAuthInvalid: ServerMessage
    {
        public string message;
    }

}