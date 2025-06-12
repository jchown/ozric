using System.Threading.Tasks;
using OzricEngine;

namespace OzricEngineTests
{
    public class MockComms : Comms
    {
        public MockComms() : base("test-comms")
        {
        }

        public override async Task<TServerResult> SendCommand<TServerResult>(ClientCommand command, int millisecondsTimeout)
        {
            var result = new TServerResult();
            result.success = true;
            result.id = command.id;
            return await Task.FromResult(result);
        }
    }
}