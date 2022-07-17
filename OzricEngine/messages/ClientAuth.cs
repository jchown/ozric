namespace OzricEngine
{
    public class ClientAuth
    {
        public ClientAuth()
        {
        }

        public ClientAuth(string accessToken)
        {
            access_token = accessToken;
        }

        public string type { get; set; } = "auth";

        public string? access_token { get; set; }
    }
}