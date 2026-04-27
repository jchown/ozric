using System;

namespace Ozric.Engine;

/// <summary>
/// What nodes use to enqueue commands during OnUpdate. Implementations live outside the engine
/// (typically in the service layer) so the engine itself stays free of HA IO.
/// </summary>
public interface ICommandSink
{
    void Add(ClientCommand command, Action<ServerResult> resultHandler);
}
