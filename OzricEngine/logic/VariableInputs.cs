using System.Collections.Generic;
using System.Linq;

namespace OzricEngine.logic
{
    public abstract class VariableInputs<T>: Node
    {
        public VariableInputs(string id, List<Pin> outputs): base(id, null, outputs)
        {
        }

        public void AddInput(string name, T t)
        {
            inputs.Add(new Pin(name, t));            
        }

        protected IEnumerable<T> GetInputValues()
        {
            return inputs.Select(input => (T) input.value);
        }
    }
}