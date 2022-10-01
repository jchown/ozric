using System;
using System.Threading.Tasks;

namespace OzricEngine;

/// <summary>
/// Simple fire-and-forget interface to sending client messages
/// </summary>
public interface ICommandSender
{
    Task<ServerResult> Send(ClientCommand command);
}
