using Ozric.Engine.Live;
using Ozric.Engine.Tests;
using Ozric.Engine.Values;

namespace Ozric.Engine.Nodes
{
    public class MockContext : Context
    {
        public MockContext(MockEngine engine): base(engine.home, new MockCommandSink(), OnPinChanged, OnAlertChanged)
        {
        }

        public MockContext(Home home): base(home, new MockCommandSink(), OnPinChanged, OnAlertChanged)
        {
        }

        private static void OnPinChanged(string nodeid, string pinname, Value value)
        {
        }

        private static void OnAlertChanged(string nodeid)
        {
        }
    }
}
