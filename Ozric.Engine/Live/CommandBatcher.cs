using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozric.Engine.Graph;

namespace Ozric.Engine.Live;

/// <summary>
/// Collate all commands we want to send, so we can batch them where possible
/// </summary>

public class CommandBatcher: OzricObject
{
    public override string Name => "CommandSender";

    public readonly List<ClientCommand> commands = new();
    private readonly Dictionary<int, List<Action<ServerResult>>> handlers = new();
        
    private const int COMMAND_TIMEOUT_MS = 5000;

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

                    if (om.Merge(command))
                    {
                        handlers[oc.id].Add(resultHandler);
                        return;
                    }
                }    
            }
            
            handlers[command.id] = new List<Action<ServerResult>> { resultHandler };
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
            handlersToProcess = new Dictionary<int, List<Action<ServerResult>>>(handlers);
            handlers.Clear();
        }

        foreach (var command in commandsToSend)
        {
            tasks[command.id] = comms.SendCommand<ServerResult>(command, COMMAND_TIMEOUT_MS);
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