using Blazor.Diagrams.Core.Geometry;
using JetBrains.Annotations;
using OzricEngine.logic;

namespace OzricUI.Model;

public class IfAnyModel: VariableInputsModel
{
    public IfAnyModel(IfAny ifAny, Point? point = null): base(ifAny, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:gate-or";
}