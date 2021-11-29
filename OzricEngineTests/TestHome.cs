using System;
using System.Collections.Generic;
using System.Linq;
using OzricEngine;
using OzricEngine.logic;

namespace OzricEngineTests
{
    public class TestHome: Home
    {
        private readonly DateTime time;

        public TestHome(List<State> stateList) : base(stateList)
        {
        }

        public TestHome(DateTime time, params string[] testEntities) : base(testEntities.Select(TestState.Load).ToList())
        {
            this.time = time;
        }

        public override DateTime GetTime()
        {
            return time;
        }
    }
}