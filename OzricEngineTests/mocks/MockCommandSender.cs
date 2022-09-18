using System;
using System.Collections.Generic;
using System.IO;
using OzricEngine.ext;

namespace OzricEngine.Nodes
{
    public class MockCommandSender : CommandSender
    {
        public MockCommandSender(params string[] events)
        {
            Array.ForEach(events, (ev) => Add(LoadMockCommand(ev), (e) => { }));
        }

        public List<ClientCommand> GetCommands()
        {
            return commands;
        }
        
        public ClientCommand LoadMockCommand(string name)
        {
            var json = File.ReadAllText($"../../../commands/{name}.json");
            try
            {
                return Json.Deserialize<ClientCommand>(json);
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing commands/{name}.json");
            }
        }

    }
}