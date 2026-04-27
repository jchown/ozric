using System.Threading.Tasks;

namespace Ozric.Engine;

/// <summary>
/// Simple fire-and-forget interface to sending client messages
/// </summary>
public interface ICommandSender
{
    Task<ServerResult> Send(ClientCommand command);
}
