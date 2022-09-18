using OzricEngineTests;

namespace OzricEngine.Nodes
{
    public class MockContext : Context
    {
        public MockContext(MockEngine engine): base(engine, new MockCommandSender())
        {
        }
    }
}