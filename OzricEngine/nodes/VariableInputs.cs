using System.Collections.Generic;
using System.Linq;

namespace OzricEngine.logic
{
    public abstract class VariableInputs: Node
    {
        private readonly ValueType valueType;

        public VariableInputs(string id, ValueType valueType, List<Pin> outputs): base(id, null, outputs)
        {
            this.valueType = valueType;
        }

        public void AddInput(string name)
        {
            inputs.Add(new Pin(name, valueType));            
        }

        protected IEnumerable<TValue> GetInputValues<TValue>() where TValue: Value
        {
            return inputs.Select(input => input.value as TValue);
        }
    }
}