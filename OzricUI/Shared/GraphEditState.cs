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

    public EditMode mode { get; set; } = EditMode.View;
    
    public bool IsEditing => mode != EditMode.View;
    public bool IsChanged { get; set; }
    public bool CanUndo { get; set; }
    public bool CanRedo { get; set; }

    public void OnEdit()
    {
        mode = EditMode.EditOffline;
    }

    public void OnCancel()
    {
        mode = EditMode.View;
    }
}