namespace OzricEngine
{
    /// <summary>
    /// Base class for response messages from the server.
    /// </summary>
    public abstract class ServerResponse: ServerMessage
    {
        public int? id { get; set; }
        public bool success { get; set; }
        
        public ServerErrorResponse? error { get; set; }
        
        public string Describe()
        {
            if (success)
                return "Success";
            
            if (error == null)
                return "Unknown error";

            return $"{error.message} (error code {error.code})";
        }
    }
}