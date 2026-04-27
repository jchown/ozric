using System;
using System.Collections.Generic;
using System.Linq;
using Ozric.Engine.Model;

namespace Ozric.Engine.Tests
{
    public class MockHome: Home
    {
        private DateTime _time;
        private readonly Dictionary<string, EntityState> _states;

        public MockHome(List<EntityState> stateList): base(stateList)
        {
            _time = new DateTime();
        }

        public MockHome(DateTime time, params string[] testEntities) : this(testEntities.Select(MockStates.Load).ToList())
        {
            _time = time;
        }

        public DateTime GetTime()
        {
            return _time;
        }

        public void SetTime(DateTime time)
        {
            _time = time;
        }
    }
}