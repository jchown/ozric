using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.logic;

namespace OzricEngine
{
    public class CommandSender: OzricObject, Engine.ICommandSender
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
                    foreach (var otherCommand in commands)
                    {
                        if (otherCommand is IMergable otherMergable)
                        {
                            if (otherMergable.Merge(command))
                            {
                                handlers[otherCommand.id].Add(resultHandler);
                                return;
                            }
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
            
            foreach (var command in commands)
            {
                tasks[command.id] = comms.SendCommand(command, COMMAND_TIMEOUT_MS);
            }

            foreach (var task in tasks)
            {
                var result = await task.Value;
                
                foreach (var handler in handlers[task.Key])
                {
                    handler.Invoke(result);
                }
            }
        }

        public override string Name => "CommandSender";
    }
}