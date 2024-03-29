using Blazor.Diagrams.Core.Models.Base;
using Microsoft.AspNetCore.Components.Web;
using OzricEngine.Nodes;
using OzricEngine.Values;

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

    public enum Command
    {
        Undo, Redo, SetCheckpoint, Delete,
        Rebuild
    }
    
    /// <summary>
    /// Events sent to GraphEditor
    /// </summary>

    public event Action<Command>? OnDoCommand;
    public event Action<GraphEditAction>? OnDoAction;
    public event Action<string>? OnEntityRefresh;
    public event Action<string, string, Value>? OnPinChanged;
    public event Action<string>? OnAlertChanged;

    /// <summary>
    /// Events sent by GraphEditor
    /// </summary>

    public event Action? OnChanged;
    public event Action<List<KeyValuePair<SelectableModel, IGraphObject>>>? OnSelectionChanged; 
    public event Action<KeyboardEventArgs>? OnKeyDown;

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

    public void DoCommand(Command command)
    {
        OnDoCommand?.Invoke(command);
    }

    public void DoAction(GraphEditAction editAction)
    {
        OnDoAction?.Invoke(editAction);
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

    public void KeyDown(KeyboardEventArgs kea)
    {
        OnKeyDown?.Invoke(kea);
    }
    
    public void RefreshEntity(string entityID)
    {
        OnEntityRefresh?.Invoke(entityID);
    }

    public void PinChanged(string nodeID, string pinName, Value value)
    {
        OnPinChanged?.Invoke(nodeID, pinName, value);
    }
    
    public void AlertChanged(string nodeID)
    {
        OnAlertChanged?.Invoke(nodeID);
    }
    
    public bool IsLocked()
    {
        switch (Mode)
        {
            case EditMode.View:
            case EditMode.Saving:
                return true;

            case EditMode.EditOffline:
            case EditMode.EditOnline:
                return false;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}