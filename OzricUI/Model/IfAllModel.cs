using Blazor.Diagrams.Core.Geometry;
using JetBrains.Annotations;
using OzricEngine.logic;

namespace OzricUI.Model;

public class IfAllModel: VariableInputsModel
{
    public IfAllModel(IfAll ifAll, Point? point = null): base(ifAll, point)
    {
        AddPort(new OutputOnOff(IfAll.OUTPUT_NAME, this));
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:gate-and";
}