using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Switch<T>: Node where T: new()
    {
        public Switch(string id): base(id, new List<Pin> { new Pin("on", new T()), new Pin("off", new T()), new Pin("switch", new OnOff()) }, new List<Pin> { new Pin("output", new T()) })
        {
        }
            
        public override void OnInit(Engine engine)
        {
            UpdateValue();
        }

        public override void OnUpdate(Engine engine)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            var switcher = GetInput("switch").value as OnOff ?? throw new Exception("No 'switch' found");

            if (switcher.value)
            {
                SetOutputValue("output", GetInput("on").value);
            }
            else
            {
                SetOutputValue("output", GetInput("off").value);
            }
        }
    }
}