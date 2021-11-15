namespace OzricEngine
{
    public abstract class ServerResult
    {
        public string id { get; set; }
        public string type { get; set; }

        public enum Type
        {
            auth_ok, auth_invalid
        }
    }
}