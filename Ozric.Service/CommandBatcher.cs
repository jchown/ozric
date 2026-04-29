using Ozric.Engine;
using Ozric.Engine.Graph;

namespace Ozric.Service;

/// <summary>
/// Collate all commands we want to send, so we can batch them where possible
/// </summary>

public class CommandBatcher: OzricObject, ICommandSink
{
    public override string Name => "CommandSender";

    public readonly List<ClientCommand> commands = new();

    private const int CommandTimeoutMs = 5000;

    private readonly Dictionary<int, List<Action<ServerResult>>> _handlers = new();

    public void Add(ClientCommand command, Action<ServerResult> resultHandler)
    {
        lock (commands)
        {
            if (command is IMergable)
            {
                foreach (var oc in commands)
                {
                    if (oc is not IMergable om)
                        continue;

                    if (!om.Merge(command))
                        continue;
                    
                    _handlers[oc.id].Add(resultHandler);
                    return;
                }
            }

            _handlers[command.id] = [resultHandler];
            commands.Add(command);
        }
    }

    public async Task Send(IComms comms)
    {
        var tasks = new Dictionary<int, Task<ServerResult>>();

        ClientCommand[] commandsToSend;
        Dictionary<int, List<Action<ServerResult>>> handlersToProcess;

        lock (commands)
        {
            commandsToSend = commands.ToArray();
            commands.Clear();
            handlersToProcess = new Dictionary<int, List<Action<ServerResult>>>(_handlers);
            _handlers.Clear();
        }

        foreach (var command in commandsToSend)
        {
            tasks[command.id] = comms.SendCommand<ServerResult>(command, CommandTimeoutMs);
        }

        foreach (var task in tasks)
        {
            var result = await task.Value;
            if (!handlersToProcess.TryGetValue(task.Key, out var handlersList))
                continue;

            foreach (var handler in handlersList)
                handler.Invoke(result);
        }
    }
}
