using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Microsoft.AspNetCore.Components.Web;
using OzricEngine.logic;
using OzricUI.Components;

namespace OzricUI.Shared;

public class EditHistory
{
    public interface GraphAction
    {
        void Undo(GraphEditor editor);
        void Do(GraphEditor editor);

        record AddNodes(List<Func<Node>> nodeCreators): GraphAction
        {
            public void Do(GraphEditor editor)
            {
                //  Node IDs are currently deterministic so re-recording IDs is defensive only
                
                nodeIDs = editor.CreateNodes(nodeCreators);
            }
            
            public void Undo(GraphEditor editor)
            {
                foreach (var nodeID in nodeIDs)
                {
                    var node = editor.Graph.GetNode(nodeID);
                    editor.RemoveNode(node);
                }
            }

            private List<string> nodeIDs = new();
        }

        record AddNode(NodeModel node): GraphAction
        {
            public void Do(GraphEditor editor)
            {
                editor.diagram.Nodes.Add(node);
            }

            public void Undo(GraphEditor editor)
            {
                editor.diagram.Nodes.Remove(node);
            }
        }

        record MoveNode(NodeModel node, Point @from, Point to): GraphAction
        {
            public void Do(GraphEditor editor)
            {
                node.SetPosition(to.X, to.Y);
            }

            public void Undo(GraphEditor editor)
            {
                node.SetPosition(from.X, from.Y);
            }
            
            public GraphAction WithTo(Point to)
            {
                return new MoveNode(node, from, to);
            }
        }

        record RemoveNode(Node node): GraphAction
        {
            private Point position = Point.Zero;

            public void Do(GraphEditor editor)
            {
                position = editor.GetPosition(node);
                editor.RemoveNode(node);
            }

            public void Undo(GraphEditor editor)
            {
                editor.AddNode(node, position);
            }
        }
        
        record AddLink(BaseLinkModel link): GraphAction
        {
            public void Undo(GraphEditor editor)
            {
                editor.diagram.Links.Remove(link);
            }

            public void Do(GraphEditor editor)
            {
                editor.diagram.Links.Add(link);
            }
        }
        
        record RemoveLink(BaseLinkModel link): GraphAction
        {
            public void Undo(GraphEditor editor)
            {
                editor.diagram.Links.Add(link);
            }

            public void Do(GraphEditor editor)
            {
                editor.diagram.Links.Remove(link);
            }
        }
    }
    
    private readonly GraphEditor editor;
    private readonly List<GraphAction> undoActionList;
    private readonly List<GraphAction> redoActionList;
    private int actionHistoryMaxSize = 200;
    private bool isTrackingHistory, isDoing;
    private int checkpoint = 0;

    public EditHistory(GraphEditor editor)
    {
        this.editor = editor;

        undoActionList = new();
        redoActionList = new();
        isTrackingHistory = true;

        editor.diagram.KeyDown += KeyboardHandle;
        editor.diagram.Links.Added += Links_Added;
        editor.diagram.Links.Removed += Links_Removed;
        editor.diagram.Nodes.Added += Nodes_Added;
//        editor.diagram.Nodes.Removed += Nodes_Removed;
    }

    private void KeyboardHandle(KeyboardEventArgs e)
    {
        if (e.CtrlKey && e.Key.Equals("z"))
        {
            UndoLastAction();
        }
        else if (e.CtrlKey && e.Key.Equals("y"))
        {
            RedoLastAction();
        }
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

    private void RegisterUndoHistoryAction(GraphAction action)
    {
        if (!isTrackingHistory)
            return;

        ClearRedoList();

        if (undoActionList.Count > actionHistoryMaxSize)
        {
            undoActionList.RemoveAt(0);
            checkpoint--;
        }

        undoActionList.Add(action);
    }

    private void Links_Added(BaseLinkModel link)
    {
        if (link.TargetNode is null)
            link.TargetPortChanged += Link_Connected; //In case its a empty link being dragged (listen for its connection)
        else
            //In case it was connected instantaneously (via code)
            RegisterUndoHistoryAction(new GraphAction.AddLink(link)); 
    }

       
    private void Link_Connected(BaseLinkModel arg1, PortModel? _, PortModel? outPort)
    {
        arg1.SourcePortChanged -= Link_Connected;
        RegisterUndoHistoryAction(new GraphAction.AddLink(arg1));
    }

    private void Links_Removed(BaseLinkModel link)
    {
        if (link.IsAttached)
            RegisterUndoHistoryAction(new GraphAction.RemoveLink(link));
    }

    private void Nodes_Added(NodeModel node)
    {
        RegisterUndoHistoryAction(new GraphAction.AddNode(node));
    }

    /*
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

            if (undoActionList.Last() is GraphAction.MoveNode lastMove)
            {
                if (lastMove.node == node)
                {
                    undoActionList[^1] = lastMove.WithTo(to);
                    return;
                }
            }
        }
        
        RegisterUndoHistoryAction(new GraphAction.MoveNode(node, from, to));
    }

    public void SetCheckpoint()
    {
        checkpoint = undoActionList.Count;
    }

    public bool IsAtCheckpoint()
    {
        return checkpoint == undoActionList.Count;
    }

    public void Record(Func<GraphAction> action)
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
    
    public void Do(GraphAction action)
    {
        if (isDoing)
            throw new Exception("Cannot nest history recording");
        
        isDoing = true;
        try
        {
            action.Do(editor);
            RegisterUndoHistoryAction(action);
        }
        finally
        {
            isDoing = false;
        }
    }
}