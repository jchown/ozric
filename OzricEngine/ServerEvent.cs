namespace OzricEngine
{
    [ServerResultType("event")]
    public class ServerEvent: ServerResult
    {
        public int id;
        public string type;
    }
}