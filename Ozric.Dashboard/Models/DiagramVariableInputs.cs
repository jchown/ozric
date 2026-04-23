using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Ozric.Engine.Graph;

namespace Ozric.Dashboard.Model;

public abstract class DiagramVariableInputs: DiagramNode
{
    public PortInputAdd plus;

    public DiagramVariableInputs(OzricEngine.Nodes.GraphVariableInputs node, Point? point = null) : base(node, point)
    {
        plus = new PortInputAdd(this, node.valueType, PortAlignment.BottomLeft);
        AddPort(plus);
        _inputLabels = false;
    }

    public void AddVariableInput(Pin pin)
    {
        // Make sure the "add" port is always last
        
        RemovePort(plus);
        base.AddInput(pin);
        AddPort(plus);
    }

    public override int PortHeight()
    {
        return 1;
    }

    public override int GetPortPosition(IPort port)
    {
        return 0;
    }
}