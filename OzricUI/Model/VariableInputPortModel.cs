using Blazor.Diagrams.Core.Models;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public class VariableInputPortModel : PortModel, IValueInput
{
    public ValueType valueType { get; }

    public VariableInputPortModel(VariableInputsModel model, ValueType valueType, PortAlignment alignment) : base(model, alignment)
    {
        this.valueType = valueType;
    }

    public override bool CanAttachTo(PortModel port)
    {
        return port is OutputPortModel opm && opm.valueType == valueType;
    }
}