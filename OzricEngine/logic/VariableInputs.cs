using System.Collections.Generic;
using System.Linq;

namespace OzricEngine.logic
{
    public abstract class VariableInputs<TValue>: Node where TValue: Value, new()
    {
        public VariableInputs(string id, List<Pin> outputs): base(id, null, outputs)
        {
        }

        public void AddInput(string name, TValue t)
        {
            inputs.Add(new Pin(name, t));            
        }

        protected IEnumerable<TValue> GetInputValues()
        {
            return inputs.Select(input => (TValue) input.value);
        }
    }
}