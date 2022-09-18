using System;
using System.Collections.Generic;
using System.Linq;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

public abstract class VariableInputs: Node
{
    public ValueType valueType { get; }

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
        
    #region Comparison
    public override bool Equals(object? obj)
    {
        if (!(obj is VariableInputs vi))
            return false;
        
        return base.Equals(obj) && valueType == vi.valueType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetHashCode(), valueType);
    }
    #endregion

}