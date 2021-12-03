using System.Collections.Generic;
using System.Linq;

namespace OzricEngine.logic
{
    public abstract class VariableInputs<T>: Node
    {
        public VariableInputs(string id, List<Output> outputs): base(id, new List<Input>(), outputs)
        {
        }

        protected IEnumerable<T> getInputValues()
        {
            return inputs.Select(input => (T) input.value);
        }
    }
}