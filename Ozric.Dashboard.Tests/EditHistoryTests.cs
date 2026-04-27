using Blazor.Diagrams.Core.Models;
using Ozric.Dashboard.Components;
using Ozric.Dashboard.Shared;
using Xunit;

namespace Ozric.Dashboard.Tests;

public class EditHistoryTests
{
    private sealed class StubAction : GraphEditAction
    {
        public int DoCount;
        public int UndoCount;
        public Action<AreaGraphView?>? DoImpl;
        public Action<AreaGraphView?>? UndoImpl;

        public void Do(AreaGraphView editor)
        {
            DoCount++;
            DoImpl?.Invoke(editor);
        }

        public void Undo(AreaGraphView editor)
        {
            UndoCount++;
            UndoImpl?.Invoke(editor);
        }
    }

    private static EditHistory NewHistory() => new(null!);

    // ---- Basic bookkeeping ----

    [Fact]
    public void NewHistory_IsEmpty()
    {
        var h = NewHistory();
        Assert.False(h.CanUndo());
        Assert.False(h.CanRedo());
        Assert.True(h.IsAtCheckpoint());
    }

    [Fact]
    public void Do_InvokesDo_AndEnablesUndo()
    {
        var h = NewHistory();
        var a = new StubAction();

        h.Do(a);

        Assert.Equal(1, a.DoCount);
        Assert.True(h.CanUndo());
        Assert.False(h.CanRedo());
    }

    [Fact]
    public void Undo_InvokesUndo_AndEnablesRedo()
    {
        var h = NewHistory();
        var a = new StubAction();
        h.Do(a);

        h.UndoLastAction();

        Assert.Equal(1, a.UndoCount);
        Assert.False(h.CanUndo());
        Assert.True(h.CanRedo());
    }

    [Fact]
    public void Redo_InvokesDoAgain()
    {
        var h = NewHistory();
        var a = new StubAction();
        h.Do(a);
        h.UndoLastAction();

        h.RedoLastAction();

        Assert.Equal(2, a.DoCount);
        Assert.Equal(1, a.UndoCount);
        Assert.True(h.CanUndo());
        Assert.False(h.CanRedo());
    }

    [Fact]
    public void NewDo_AfterUndo_ClearsRedo()
    {
        var h = NewHistory();
        var a = new StubAction();
        h.Do(a);
        h.UndoLastAction();
        Assert.True(h.CanRedo());

        h.Do(new StubAction());

        Assert.False(h.CanRedo());
    }

    [Fact]
    public void UndoEmpty_IsNoOp()
    {
        var h = NewHistory();

        h.UndoLastAction();

        Assert.False(h.CanUndo());
        Assert.False(h.CanRedo());
    }

    [Fact]
    public void RedoEmpty_IsNoOp()
    {
        var h = NewHistory();

        h.RedoLastAction();

        Assert.False(h.CanRedo());
    }

    // ---- Exception safety ----

    [Fact]
    public void Undo_ResetsDoingFlag_OnException()
    {
        var h = NewHistory();
        var throwing = new StubAction
        {
            UndoImpl = _ => throw new InvalidOperationException("boom")
        };
        h.Do(throwing);

        Assert.Throws<InvalidOperationException>(() => h.UndoLastAction());

        // After the throw, _isDoing must be reset. A subsequent Do must not
        // trip the "Cannot nest history recording" guard.
        var follow = new StubAction();
        h.Do(follow);
        Assert.Equal(1, follow.DoCount);
    }

    [Fact]
    public void Redo_ResetsDoingFlag_OnException()
    {
        var h = NewHistory();
        var a = new StubAction();
        h.Do(a);
        h.UndoLastAction();

        a.DoImpl = _ => throw new InvalidOperationException("boom");
        Assert.Throws<InvalidOperationException>(() => h.RedoLastAction());

        a.DoImpl = null;
        var follow = new StubAction();
        h.Do(follow);
        Assert.Equal(1, follow.DoCount);
    }

    [Fact]
    public void NestedDoInsideDo_Throws()
    {
        // Recording while applying an action is misuse; keep the guard.
        var h = NewHistory();
        var outer = new StubAction();
        outer.DoImpl = _ => h.Do(new StubAction());
        Assert.Throws<Exception>(() => h.Do(outer));
    }

    // ---- Checkpoint identity ----

    [Fact]
    public void Checkpoint_Fresh_IsAtCheckpoint()
    {
        var h = NewHistory();
        Assert.True(h.IsAtCheckpoint());
    }

    [Fact]
    public void Checkpoint_AtCheckpoint_AfterSave()
    {
        var h = NewHistory();
        h.Do(new StubAction());
        h.Do(new StubAction());

        h.SetCheckpoint();

        Assert.True(h.IsAtCheckpoint());
    }

    [Fact]
    public void Checkpoint_AtCheckpoint_AfterUndoAndRedo()
    {
        var h = NewHistory();
        h.Do(new StubAction());
        h.SetCheckpoint();

        h.UndoLastAction();
        h.RedoLastAction();

        Assert.True(h.IsAtCheckpoint());
    }

    [Fact]
    public void Checkpoint_AtCheckpoint_AfterUndoingNewEdits()
    {
        var h = NewHistory();
        h.Do(new StubAction());
        h.Do(new StubAction());
        h.SetCheckpoint();

        h.Do(new StubAction());
        h.Do(new StubAction());
        h.UndoLastAction();
        h.UndoLastAction();

        Assert.True(h.IsAtCheckpoint());
    }

    [Fact]
    public void Checkpoint_NotAtCheckpoint_AfterUndoThenDifferentDo()
    {
        // After saving, undo the saved action, then do a *different* action.
        // The new list has the same count as at save, but the state is not
        // the saved state — IsAtCheckpoint must be false.
        var h = NewHistory();
        h.Do(new StubAction());
        h.SetCheckpoint();
        Assert.True(h.IsAtCheckpoint());

        h.UndoLastAction();
        Assert.False(h.IsAtCheckpoint());

        h.Do(new StubAction());
        Assert.False(h.IsAtCheckpoint());
    }

    // ---- Move compression ----

    [Fact]
    public void NodeMoved_SameNode_CompressesInPlace()
    {
        var h = NewHistory();
        var node = new NodeModel();

        h.Node_Moved(node, "n1", new LayoutPoint(0, 0), new LayoutPoint(1, 1));
        h.Node_Moved(node, "n1", new LayoutPoint(1, 1), new LayoutPoint(2, 2));

        Assert.Equal(1, h.UndoCount);
    }

    [Fact]
    public void NodeMoved_DifferentNode_AppendsNewEntry()
    {
        var h = NewHistory();
        var n1 = new NodeModel();
        var n2 = new NodeModel();

        h.Node_Moved(n1, "n1", new LayoutPoint(0, 0), new LayoutPoint(1, 1));
        h.Node_Moved(n2, "n2", new LayoutPoint(0, 0), new LayoutPoint(1, 1));

        Assert.Equal(2, h.UndoCount);
    }

    [Fact]
    public void NodeMoved_SkippedDuringActionApply()
    {
        // Side-effect move events fired during Do/Undo must not be recorded.
        var h = NewHistory();
        var node = new NodeModel();
        var action = new StubAction();
        action.DoImpl = _ => h.Node_Moved(
            node, "n1", new LayoutPoint(0, 0), new LayoutPoint(1, 1));

        h.Do(action);

        // Only the action itself is on the stack, not the inner move.
        h.UndoLastAction();
        Assert.False(h.CanUndo());
        Assert.Equal(1, action.UndoCount);
    }

    [Fact]
    public void NodesMoved_SameSet_CompressesInPlace()
    {
        var h = NewHistory();
        var n1 = new NodeModel();
        var n2 = new NodeModel();

        h.Nodes_Moved(new List<GraphEditAction.MoveNode>
        {
            new(n1, "n1", new LayoutPoint(0, 0), new LayoutPoint(1, 1)),
            new(n2, "n2", new LayoutPoint(0, 0), new LayoutPoint(1, 1)),
        });
        h.Nodes_Moved(new List<GraphEditAction.MoveNode>
        {
            new(n1, "n1", new LayoutPoint(1, 1), new LayoutPoint(2, 2)),
            new(n2, "n2", new LayoutPoint(1, 1), new LayoutPoint(2, 2)),
        });

        Assert.Equal(1, h.UndoCount);
    }

    [Fact]
    public void NodesMoved_DifferentSet_AppendsNewEntry()
    {
        var h = NewHistory();
        var n1 = new NodeModel();
        var n2 = new NodeModel();

        h.Nodes_Moved(new List<GraphEditAction.MoveNode>
        {
            new(n1, "n1", new LayoutPoint(0, 0), new LayoutPoint(1, 1)),
        });
        h.Nodes_Moved(new List<GraphEditAction.MoveNode>
        {
            new(n1, "n1", new LayoutPoint(1, 1), new LayoutPoint(2, 2)),
            new(n2, "n2", new LayoutPoint(0, 0), new LayoutPoint(1, 1)),
        });

        Assert.Equal(2, h.UndoCount);
    }

    // ---- History size limit ----

    [Fact]
    public void HistoryLimit_EvictsOldestEntries()
    {
        var h = NewHistory();
        var first = new StubAction();
        h.Do(first);

        for (int i = 0; i < 250; i++)
            h.Do(new StubAction());

        int undos = 0;
        while (h.CanUndo() && undos < 400)
        {
            h.UndoLastAction();
            undos++;
        }

        // The very first action should have been evicted; its Undo never runs.
        Assert.Equal(0, first.UndoCount);
        // Bounded by ~ActionHistoryMaxSize (200).
        Assert.InRange(undos, 200, 210);
    }
}
