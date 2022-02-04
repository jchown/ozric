using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.IfAny)]
    public class IfAny: VariableInputs
    {
        public override NodeType nodeType => NodeType.IfAny;

        public IfAny() : this(null)
        {
            
        }

        public IfAny(string id): base(id, ValueType.OnOff, new List<Pin> { new("output", ValueType.OnOff) })
        {
        }

        public IfAny(string id, params string[] inputNames): this(id)
        {
            foreach (var inputName in inputNames)
                AddInput(inputName);
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