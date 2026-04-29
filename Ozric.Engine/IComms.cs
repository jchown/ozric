using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ozric.Engine;

/// <summary>
/// Comms wrapper for the WebSocket API to Home Assistant.
/// </summary>
public interface IComms: IDisposable
{
    public const int DefaultCommandTimeoutMilliseconds = 10000;

    public delegate void MessageHandler(object o);

    public delegate void JsonHandler(string message);

    CommsStatus Status { get; }

    Task Connect();

    public Task<TResponse> SendCommand<TResponse>(ClientCommand command, int millisecondsTimeout = DefaultCommandTimeoutMilliseconds, bool verbose = false) where TResponse: ServerResponse, new();
        
    List<ServerEvent> TakePendingEvents(int millisToWait);

    public void OnSentMessage(MessageHandler action);

    public void OnSentJson(JsonHandler action);

    public void OnReceivedJson(JsonHandler action);
}