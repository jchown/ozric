using OzricEngineTests;

namespace OzricEngine.logic
{
    public class MockContext : Context
    {
        public MockContext(MockEngine engine): base(engine, new MockCommandSender())
        {
        }
    }
}