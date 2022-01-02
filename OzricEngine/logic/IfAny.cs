using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class Or: VariableInputs<OnOff>
    {
        public Or(string id): base(id, new List<Pin> { new Pin("output", new OnOff()) })
        {
        }
            
        public override Task OnInit(Engine engine)
        {
            UpdateValue(engine);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
        {
            UpdateValue(engine);
            return Task.CompletedTask;
        }

        private void UpdateValue(Engine engine)
        {
            var on = false;
            foreach (var onOff in GetInputValues())
                on |= onOff.value;

            var value = new OnOff(on);
            engine.Log($"{id}.output = {value}");
            SetOutputValue("output", value);
        }
    }
}