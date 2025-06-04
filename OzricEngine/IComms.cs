using System.Threading.Tasks;

namespace OzricEngine;

public interface IComms
{
    public delegate void MessageHandler(object o);

    public delegate void JsonHandler(string message);

    Task Authenticate();
    
    Task Send<T>(T t);

    public Task<ServerResult> SendCommand<T>(T command, int millisecondsTimeout) where T : ClientCommand;
    
    public void OnSentMessage(MessageHandler action);

    public void OnSentJson(JsonHandler action);

    public void OnReceivedJson(JsonHandler action);
}