public class OzricConfig
{
    public int port = 8099;
    public string url = "/";

    public string GetBaseUrl()
    {
        return url;
    }

    public int GetPort()
    {
        return port;
    }
};