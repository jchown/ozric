namespace OzricEngine
{
    [TypeKey("result")]
    public class ServerResult: ServerMessage
    {
        public int id { get; set; }
        public bool success { get; set; }
        public Result result { get; set; }
    }
}