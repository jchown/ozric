using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class LightModel: GraphNodeModel
{
    public LightModel(Light light, Point? point = null): base(light, point)
    {
        AddPort(new InputColor(Light.INPUT_NAME, this));
    }
}