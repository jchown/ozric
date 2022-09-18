using Blazor.Diagrams.Core.Models;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

/// <summary>
/// The "empty slot" input we need to connect (and implicitly add) new ports. 
/// </summary>
public class PortInputAdd : PortModel, IPort
{
    public bool hiddenIfLocked => true;
    public bool input => true;
    public string cssClass => "input-port";

    public ValueType valueType { get; }

    public PortInputAdd(VariableInputsModel model, ValueType valueType, PortAlignment alignment) : base(model, alignment)
    {
        this.valueType = valueType;
    }

    public override bool CanAttachTo(PortModel port)
    {
        return port is IPort { input: false } output && output.valueType == valueType;
    }
}