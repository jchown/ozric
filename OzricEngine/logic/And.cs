using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Or: VariableInputs<OnOff>
    {
        public Or(string id): base(id, new List<Pin> { new Pin("output", new OnOff()) })
        {
        }
            
        public override void OnInit(Home home)
        {
            UpdateValue(home);
        }

        public override void OnUpdate(Home home)
        {
            UpdateValue(home);
        }

        private void UpdateValue(Home home)
        {
            var on = false;
            foreach (var onOff in GetInputValues())
                on |= onOff.value;

            var value = new OnOff(on);
            home.Log($"{id}.output = {value}");
            SetOutputValue("output", value);
        }
    }
}