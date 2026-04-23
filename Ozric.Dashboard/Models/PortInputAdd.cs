using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Model;

/// <summary>
/// The "empty slot" input we need to connect (and implicitly add) new ports. 
/// </summary>
public class PortInputAdd : PortModel, IPort
{
    public string Name => "+";
    public bool HiddenIfLocked => true;
    public bool IsInput => true;
    public string CssClass => "input-port";
    public ValueType valueType { get; }

    public PortInputAdd(DiagramVariableInputs model, ValueType valueType, PortAlignment alignment) : base(model, alignment)
    {
        this.valueType = valueType;
    }

    public override bool CanAttachTo(ILinkable port)
    {
        return port is IPort { IsInput: false } output && output.valueType == valueType;
    }
}