using System.Collections.Generic;
using OzricEngine;

namespace OzricEngineTests
{
    public class MockEngine: Engine
    {
        public MockEngine() : base(new MockHome(new List<State>()), new MockComms())
        {
        }
        
        public MockEngine(MockHome home) : base(home, new MockComms())
        {
        }
    }
}