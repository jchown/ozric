public class OzricConfig
{
    public int Port = 8099;
    public string Url = "/";

    public string GetBaseUrl()
    {
        return Url;
    }

    public int GetPort()
    {
        return Port;
    }
};