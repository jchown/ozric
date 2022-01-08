using System;
using System.Collections.Generic;
using System.Linq;
using OzricEngine;
using OzricEngine.logic;

namespace OzricEngineTests
{
    public class MockHome: Home
    {
        private DateTime time;

        public MockHome(List<EntityState> stateList) : base(stateList)
        {
        }

        public MockHome(DateTime time, params string[] testEntities) : base(testEntities.Select(MockStates.Load).ToList())
        {
            this.time = time;
        }

        public override DateTime GetTime()
        {
            return time;
        }

        public void SetTime(DateTime time)
        {
            this.time = time;
        }
    }
}