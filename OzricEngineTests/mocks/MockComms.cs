using System.Threading.Tasks;
using OzricEngine;

namespace OzricEngineTests
{
    public class MockComms : Comms
    {
        public MockComms() : base("test-comms")
        {
        }

        public override Task<ServerResult> SendCommand<T>(T command, int millisecondsTimeout)
        {
            return Task.FromResult(ServerResult.Succeeded(command.id));
        }
    }
}