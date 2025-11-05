using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;
using Ozric.Dashboard.Components;
using LogLevel = Ozric.Engine.Utils.LogLevel;

namespace Ozric.Dashboard.Shared;

public class EditHistory(AreaGraphView areaGraphView) : OzricObject
{
    public override string Name => "History";

    private readonly List<GraphEditAction> _undoActionList = new();
    private readonly List<GraphEditAction> _redoActionList = new();
    private readonly bool _isTrackingHistory = true;

    private const int ActionHistoryMaxSize = 200;
    private bool _isDoing;
    private int _checkpoint = 0;

    public bool CanUndo()
    {
        return _undoActionList.Any();
    }
    
    public bool CanRedo()
    {
        return _redoActionList.Any();
    }

    public void UndoLastAction()
    {
        if (!_undoActionList.Any())
            return;
        
        Log(LogLevel.Debug, "Undo {0}", _undoActionList[^1]);
        _isDoing = true;
        _undoActionList[^1].Undo(areaGraphView);
        RemoveLastUndoAction();
        areaGraphView.Diagram.UnselectAll();
        _isDoing = false;
    }

    public void RedoLastAction()
    {
        if (!_redoActionList.Any())
            return;

        Log(LogLevel.Debug, "Redo {0}", _redoActionList[^1]);
        _isDoing = true;
        _redoActionList[^1].Do(areaGraphView);
        RemoveLastRedoAction();
        areaGraphView.Diagram.UnselectAll();
        _isDoing = false;
    }

    private void RemoveLastUndoAction()
    {
        if (!_undoActionList.Any())
            return;
        
        var action = _undoActionList.Last();
        _undoActionList.RemoveAt(_undoActionList.Count - 1);
        _redoActionList.Add(action);
    }
    
    private void RemoveLastRedoAction()
    {
        if (!_redoActionList.Any())
            return;
        
        var action = _redoActionList.Last();
        _redoActionList.RemoveAt(_redoActionList.Count - 1);
        _undoActionList.Add(action);
    }

    private void ClearRedoList() => _redoActionList.Clear();

    private void RegisterUndoHistoryAction(GraphEditAction editAction)
    {
        if (!_isTrackingHistory)
            return;

        ClearRedoList();

        if (_undoActionList.Count > ActionHistoryMaxSize)
        {
            _undoActionList.RemoveAt(0);
            _checkpoint--;
        }

        Log(LogLevel.Debug, "Did {0}", editAction);
        _undoActionList.Add(editAction);
    }
    
    public void Node_Moved(NodeModel node, Point from, Point to)
    {
        if (_isDoing)
            return;
        
        if (_undoActionList.Any())
        {
            //  Compress moves of the same object

            if (_undoActionList.Last() is GraphEditAction.MoveNode lastMove)
            {
                if (lastMove.Node == node)
                {
                    _undoActionList[^1] = lastMove.WithTo(to);
                    return;
                }
            }
        }
        
        RegisterUndoHistoryAction(new GraphEditAction.MoveNode(node, from, to));
    }
    
    public void Nodes_Moved(List<GraphEditAction.MoveNode> nodeMoves)
    {
        if (_isDoing)
            return;
        
        if (_undoActionList.Any())
        {
            //  Compress moves of the same objects

            if (_undoActionList.Last() is GraphEditAction.MoveNodes lastMove)
            {
                if (lastMove.Moves.Select(m => m.Node).SequenceEqual(nodeMoves.Select(m => m.Node)))
                {
                    _undoActionList[^1] = lastMove.WithTo(nodeMoves);
                    return;
                }
            }
        }
        
        RegisterUndoHistoryAction(new GraphEditAction.MoveNodes(nodeMoves));
    }

    public void SetCheckpoint()
    {
        _checkpoint = _undoActionList.Count;
    }

    public bool IsAtCheckpoint()
    {
        return _checkpoint == _undoActionList.Count;
    }

    public void Record(Func<GraphEditAction> action)
    {
        if (_isDoing)
            throw new Exception("Cannot nest history recording");
        
        _isDoing = true;
        try
        {
            var undo = action();
            RegisterUndoHistoryAction(undo);
        }
        finally
        {
            _isDoing = false;
        }
    }
    
    public void Do(GraphEditAction editAction)
    {
        if (_isDoing)
            throw new Exception("Cannot nest history recording");
        
        _isDoing = true;
        try
        {
            editAction.Do(areaGraphView);
            RegisterUndoHistoryAction(editAction);
        }
        finally
        {
            _isDoing = false;
        }
    }
}