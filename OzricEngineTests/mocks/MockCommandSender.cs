using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class MockCommandSender : CommandSender
    {
        public List<ClientCommand> GetCommands()
        {
            return commands;
        }
    }
}