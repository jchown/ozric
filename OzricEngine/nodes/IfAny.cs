using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class IfAny: VariableInputs
    {
        public IfAny(string id): base(id, ValueType.OnOff, new List<Pin> { new Pin("output", ValueType.OnOff) })
        {
        }
            
        public override Task OnInit(Context context)
        {
            UpdateValue(context);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context context)
        {
            UpdateValue(context);
            return Task.CompletedTask;
        }

        private void UpdateValue(Context engine)
        {
            var on = false;
            foreach (var onOff in GetInputValues<OnOff>())
                on |= onOff.value;

            var value = new OnOff(on);
            Log(LogLevel.Debug, "output = {0}", value);
            SetOutputValue("output", value);
        }
    }
}