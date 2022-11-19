using OzricEngine.Values;
using OzricEngineTests;

namespace OzricEngine.Nodes
{
    public class MockContext : Context
    {
        public MockContext(MockEngine engine): base(engine.home, new MockCommandBatcher(), OnPinChanged)
        {
        }

        public MockContext(Home home): base(home, new MockCommandBatcher(), OnPinChanged)
        {
        }

        private static void OnPinChanged(string nodeid, string pinname, Value value)
        {
        }
    }
}