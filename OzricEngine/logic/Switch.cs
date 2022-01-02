using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class Switch<TValue>: Node where TValue: Value, new()
    {
        public Switch(string id): base(id, new List<Pin> { new Pin("on", new TValue()), new Pin("off", new TValue()), new Pin("switch", new OnOff()) }, new List<Pin> { new Pin("output", new TValue()) })
        {
        }
            
        public override Task OnInit(Engine engine)
        {
            UpdateValue();
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
        {
            UpdateValue();
            return Task.CompletedTask;
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