using Blazor.Diagrams.Core.Geometry;
using JetBrains.Annotations;
using OzricEngine.logic;

namespace OzricUI.Model;

public class IfAnyModel: VariableInputsModel
{
    public IfAnyModel(IfAny ifAny, Point? point = null): base(ifAny, point)
    {
        AddPort(new OutputOnOff(IfAny.OUTPUT_NAME, this));
    }
    
    public override string Icon => "mdi:gate-or";
}