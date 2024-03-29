using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Nodes;

namespace OzricEngine;

/// <summary>
/// Collate all commands we want to send, so we can batch them where possible
/// </summary>

public class CommandBatcher: OzricObject
{
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

    public async Task Send(Comms comms)
    {
        Dictionary<int, Task<ServerResult>> tasks = new Dictionary<int, Task<ServerResult>>();

        ClientCommand[] _commands;

        lock (commands)
        {
            _commands = commands.ToArray();
            commands.Clear();
        }

        foreach (var command in _commands)
        {
            tasks[command.id] = comms.SendCommand(command, COMMAND_TIMEOUT_MS);
        }

        foreach (var task in tasks)
        {
            var result = await task.Value;
            if (!handlers.ContainsKey(task.Key))
                continue;
                
            foreach (var handler in handlers[task.Key])
                handler.Invoke(result);

            handlers.Remove(task.Key);
        }
    }

    public override string Name => "CommandSender";
}