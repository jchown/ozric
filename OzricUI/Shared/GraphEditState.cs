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
    public event Action<List<Func<Node>>>? OnDoAdd; 

    public EditMode Mode { get; private set; } = EditMode.View;
    
    public bool IsEditing => Mode != EditMode.View;
    public bool IsChanged { get; private set; }
    public bool CanUndo { get; private set; }
    public bool CanRedo { get; private set; }

    public List<SelectableModel> Selected { get; private set; } = new();

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

    public void DoAdd(List<Func<Node>> nodeCreators)
    {
        OnDoAdd?.Invoke(nodeCreators);
    }

    public void SetHistoryState(EditHistory history)
    {
        CanUndo = history.CanUndo();
        CanRedo = history.CanRedo();
        IsChanged = !history.IsAtCheckpoint();
        OnChanged?.Invoke();
    }
    
    public void SetSelected(List<SelectableModel> models)
    {
        Selected = models;
        OnChanged?.Invoke();
    }
}