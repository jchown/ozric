using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Or: VariableInputs<OnOff>
    {
        public Or(string id): base(id, new List<Pin> { new Pin("output", new OnOff()) })
        {
        }
            
        public override void OnInit(Engine engine)
        {
            UpdateValue(engine);
        }

        public override void OnUpdate(Engine engine)
        {
            UpdateValue(engine);
        }

        private void UpdateValue(Engine engine)
        {
            var on = false;
            foreach (var onOff in GetInputValues())
                on |= onOff.value;

            var value = new OnOff(on);
            engine.home.Log($"{id}.output = {value}");
            SetOutputValue("output", value);
        }
    }
}