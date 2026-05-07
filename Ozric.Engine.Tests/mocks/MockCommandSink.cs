using System;
using System.Collections.Generic;

namespace Ozric.Engine.Tests
{
    public class MockCommandSink : ICommandSink
    {
        public readonly List<ClientCommand> Commands = new();

        public void Add(ClientCommand command, Action<ServerResult> resultHandler)
        {
            Commands.Add(command);
        }
    }
}
