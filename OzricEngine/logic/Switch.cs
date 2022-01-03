using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class Switch: Node
    {
        public Switch(string id, ValueType type): base(id, new List<Pin> { new Pin("on", type), new Pin("off", type), new Pin("switch", ValueType.OnOff) }, new List<Pin> { new Pin("output", type) })
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