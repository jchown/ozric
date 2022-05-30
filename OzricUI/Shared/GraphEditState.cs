namespace OzricUI.Shared;

public class GraphEditState
{
    public enum EditMode
    {
        View, EditOffline, EditOnline
    }
    
    public enum OnlineMode
    {
        Realtime, Simulated
    }

    public event Action OnChanged;
    public event Action OnDoUndo;
    public event Action OnDoRedo;
    public event Action OnDoSetCheckpoint; 

    public EditMode mode { get; set; } = EditMode.View;
    
    public bool IsEditing => mode != EditMode.View;
    public bool IsChanged { get; set; }
    public bool CanUndo { get; set; }
    public bool CanRedo { get; set; }

    public void OnEdit()
    {
        mode = EditMode.EditOffline;
        OnChanged();
    }

    public void OnCancel()
    {
        mode = EditMode.View;
        OnChanged();
    }

    public void DoUndo()
    {
        OnDoUndo();
    }

    public void DoRedo()
    {
        OnDoRedo();
    }

    public void DoSetCheckpoint()
    {
        OnDoSetCheckpoint();
    }

    public void SetHistoryState(DiagramHistory history)
    {
        CanUndo = history.CanUndo();
        CanRedo = history.CanRedo();
        IsChanged = !history.IsAtCheckpoint();
        OnChanged();
    }

    public void OnLock(bool locked)
    {
        mode = locked ? EditMode.View : EditMode.EditOffline;
        OnChanged();
    }
}