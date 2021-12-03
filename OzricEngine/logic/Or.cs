using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Or: VariableInputs<OnOff>
    {
        public Or(string id): base(id, new List<Output> { new Output("output", new OnOff()) })
        {
        }
            
        public override void OnInit(Home home)
        {
            UpdateValue();
        }

        public override void OnUpdate(Home home)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            var value = false;
            
            foreach (var onOff in getInputValues())
                value |= onOff.value;

            SetOutputValue("output", new OnOff(value));
        }
    }
}