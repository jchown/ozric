using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricUI.Components;

namespace OzricUI.Shared;

public class EditHistory
{
    private readonly GraphEditor editor;
    private readonly List<GraphEditAction> undoActionList;
    private readonly List<GraphEditAction> redoActionList;
    private int actionHistoryMaxSize = 200;
    private bool isTrackingHistory, isDoing;
    private int checkpoint = 0;

    public EditHistory(GraphEditor editor)
    {
        this.editor = editor;

        undoActionList = new();
        redoActionList = new();
        isTrackingHistory = true;
    }
    
    public bool CanUndo()
    {
        return undoActionList.Any();
    }
    
    public bool CanRedo()
    {
        return redoActionList.Any();
    }

    public void UndoLastAction()
    {
        if (!undoActionList.Any())
            return;

        isDoing = true;
        undoActionList[^1].Undo(editor);
        RemoveLastUndoAction();
        editor.diagram.UnselectAll();
        isDoing = false;
    }

    public void RedoLastAction()
    {
        if (!redoActionList.Any())
            return;

        isDoing = true;
        redoActionList[^1].Do(editor);
        RemoveLastRedoAction();
        editor.diagram.UnselectAll();
        isDoing = false;
    }

    private void RemoveLastUndoAction()
    {
        if (!undoActionList.Any())
            return;
        
        var action = undoActionList.Last();
        undoActionList.RemoveAt(undoActionList.Count - 1);
        redoActionList.Add(action);
    }
    
    private void RemoveLastRedoAction()
    {
        if (!redoActionList.Any())
            return;
        
        var action = redoActionList.Last();
        redoActionList.RemoveAt(redoActionList.Count - 1);
        undoActionList.Add(action);
    }

    private void ClearRedoList() => redoActionList.Clear();

    private void RegisterUndoHistoryAction(GraphEditAction editAction)
    {
        if (!isTrackingHistory)
            return;

        ClearRedoList();

        if (undoActionList.Count > actionHistoryMaxSize)
        {
            undoActionList.RemoveAt(0);
            checkpoint--;
        }

        undoActionList.Add(editAction);
    }

    /*
    private void Nodes_Added(NodeModel node)
    {
        RegisterUndoHistoryAction(new GraphAction.AddNode(node));
    }

    private void Nodes_Removed(NodeModel node)
    {
        RegisterUndoHistoryAction(new GraphAction.RemoveNode(node));
    }
    */
    
    public void Node_Moved(NodeModel node, Point from, Point to)
    {
        if (isDoing)
            return;
        
        if (undoActionList.Any())
        {
            //  Compress moves of the same object

            if (undoActionList.Last() is GraphEditAction.MoveNode lastMove)
            {
                if (lastMove.Node == node)
                {
                    undoActionList[^1] = lastMove.WithTo(to);
                    return;
                }
            }
        }
        
        RegisterUndoHistoryAction(new GraphEditAction.MoveNode(node, from, to));
    }

    public void SetCheckpoint()
    {
        checkpoint = undoActionList.Count;
    }

    public bool IsAtCheckpoint()
    {
        return checkpoint == undoActionList.Count;
    }

    public void Record(Func<GraphEditAction> action)
    {
        if (isDoing)
            throw new Exception("Cannot nest history recording");
        
        isDoing = true;
        try
        {
            var undo = action();
            RegisterUndoHistoryAction(undo);
        }
        finally
        {
            isDoing = false;
        }
    }
    
    public void Do(GraphEditAction editAction)
    {
        if (isDoing)
            throw new Exception("Cannot nest history recording");
        
        isDoing = true;
        try
        {
            editAction.Do(editor);
            RegisterUndoHistoryAction(editAction);
        }
        finally
        {
            isDoing = false;
        }
    }
}