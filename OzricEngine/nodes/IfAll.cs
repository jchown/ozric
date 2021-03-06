using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.IfAll)]
    public class IfAll: VariableInputs
    {
        public static string OUTPUT_NAME = "output";
        
        public override NodeType nodeType => NodeType.IfAll;

        public IfAll(string id): base(id, ValueType.OnOff, new List<Pin> { new(OUTPUT_NAME, ValueType.OnOff) })
        {
        }
            
        public override Task OnInit(Context context)
        {
            UpdateValue(context.engine);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context context)
        {
            UpdateValue(context.engine);
            return Task.CompletedTask;
        }

        private void UpdateValue(Engine engine)
        {
            var on = true;
            foreach (var onOff in GetInputValues<OnOff>())
                on &= onOff.value;

            var value = new OnOff(on);
            Log(LogLevel.Debug, "output = {0}", value);
            SetOutputValue("output", value);
        }
    }
}