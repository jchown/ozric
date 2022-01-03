namespace OzricEngine
{
    [TypeKey("result")]
    public class ServerResult: ServerMessage
    {
        public int id { get; set; }
        public bool success { get; set; }
        public ServerResultError error { get; set; }
    }

    public class ServerResultError
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}