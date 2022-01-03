using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class IfAll: VariableInputs
    {
        public IfAll(string id): base(id, ValueType.OnOff, new List<Pin> { new Pin("output", ValueType.OnOff) })
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
            var on = true;
            foreach (var onOff in GetInputValues<OnOff>())
                on &= onOff.value;

            var value = new OnOff(on);
            engine.Log($"{id}.output = {value}");
            SetOutputValue("output", value);
        }
    }
}