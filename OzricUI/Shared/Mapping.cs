using OzricEngine.Nodes;

namespace OzricUI.Shared;

public class Mapping<GType, DType> where GType: IGraphObject where DType: Blazor.Diagrams.Core.Models.Base.Model
{
    private Dictionary<string, GType> idToGType = new();
    private Dictionary<string, DType> idToDType = new();

    public void Add(GType g, DType d)
    {
        idToDType[g.id] = d;
        idToGType[d.Id] = g;
    }

    public DType Remove(GType g)
    {
        var d = idToDType[g.id];
        idToDType.Remove(g.id);
        idToGType.Remove(d.Id);
        return d;
    }

    public DType GetDiagram(GType g)
    {
        return GetDiagram(g.id);
    }

    public DType GetDiagram(string id)
    {
        return idToDType[id];
    }

    public GType GetGraph(DType d)
    {
        return GetGraph(d.Id);
    }

    public GType GetGraph(string id)
    {
        return idToGType[id];
    }

    public void Clear()
    {
        idToGType.Clear();
        idToDType.Clear();
    }

    public void Remap(GType g, string newID)
    {
        var oldID = g.id;
        var d = idToDType[oldID];
        idToDType.Remove(oldID);
        idToDType[newID] = d;
    }

    public bool HasDiagramModel(DType d)
    {
        return idToGType.ContainsKey(d.Id);
    }
}
