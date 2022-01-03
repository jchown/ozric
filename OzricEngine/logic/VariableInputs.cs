using System.Collections.Generic;
using System.Linq;

namespace OzricEngine.logic
{
    public abstract class VariableInputs: Node
    {
        private readonly ValueType type;

        public VariableInputs(string id, ValueType type, List<Pin> outputs): base(id, null, outputs)
        {
            this.type = type;
        }

        public void AddInput(string name)
        {
            inputs.Add(new Pin(name, type));            
        }

        protected IEnumerable<TValue> GetInputValues<TValue>() where TValue: Value
        {
            return inputs.Select(input => input.value as TValue);
        }
    }
}