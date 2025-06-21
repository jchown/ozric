namespace OzricEngine
{
    /// <summary>
    /// A generic untyped result message from the server. 
    /// </summary>
    [TypeKey("result")]
    public class ServerResult: ServerResponse
    {
        public static ServerResult Succeeded(int id)
        {
            return new ServerResult
            {
                id = id,
                success = true
            };
        }
    }
}