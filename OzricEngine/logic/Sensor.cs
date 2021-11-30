using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Sensor : Node
    {
        public Sensor(string id) : base(id, new List<Output> { new Output("activity", new OnOff()) })
        {
        }

        public override void OnInit(Home home)
        {
            throw new NotImplementedException();
        }

        public override void OnUpdate(Home home)
        {
            throw new NotImplementedException();
        }
    }
}