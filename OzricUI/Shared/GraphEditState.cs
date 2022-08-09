using Blazor.Diagrams.Core.Models.Base;
using OzricEngine.logic;

namespace OzricUI.Shared;

public class GraphEditState
{
    public enum EditMode
    {
        View, Saving, EditOffline, EditOnline
    }
    
    public enum OnlineMode
    {
        Realtime, Simulated
    }

    public event Action? OnChanged;
    public event Action? OnDoUndo;
    public event Action? OnDoRedo;
    public event Action? OnDoSetCheckpoint;
    public event Action<Node>? OnDoAdd; 
    public event Action<List<KeyValuePair<SelectableModel, IGraphObject>>>? OnSelectionChanged; 

    public EditMode Mode { get; private set; } = EditMode.View;
    
    public bool IsEditing => Mode != EditMode.View;
    public bool IsChanged { get; private set; }
    public bool CanUndo { get; private set; }
    public bool CanRedo { get; private set; }

    public void OnEdit()
    {
        Mode = EditMode.EditOffline;
        OnChanged?.Invoke();
    }

    public void OnSaving()
    {
        Mode = EditMode.Saving;
        OnChanged?.Invoke();
    }

    public void OnCancel()
    {
        Mode = EditMode.View;
        OnChanged?.Invoke();
    }

    public void DoUndo()
    {
        OnDoUndo?.Invoke();
    }

    public void DoRedo()
    {
        OnDoRedo?.Invoke();
    }

    public void DoSetCheckpoint()
    {
        OnDoSetCheckpoint?.Invoke();
    }

    public void DoAdd(Node node)
    {
        OnDoAdd?.Invoke(node);
    }

    public void SetHistoryState(EditHistory history)
    {
        CanUndo = history.CanUndo();
        CanRedo = history.CanRedo();
        IsChanged = !history.IsAtCheckpoint();
        OnChanged?.Invoke();
    }
    
    public void SetSelected(List<KeyValuePair<SelectableModel, IGraphObject>> models)
    {
        OnSelectionChanged?.Invoke(models);
    }
}