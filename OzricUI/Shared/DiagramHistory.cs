using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Microsoft.AspNetCore.Components.Web;

namespace OzricUI.Shared;

public class DiagramHistory
{
    public interface GraphAction
    {
        void Undo(Diagram diagram);
        void Redo(Diagram diagram);

        record AddNode(NodeModel node): GraphAction
        {
            public void Undo(Diagram diagram)
            {
                diagram.Nodes.Remove(node);
            }

            public void Redo(Diagram diagram)
            {
                diagram.Nodes.Add(node);
            }
        }

        record MoveNode(NodeModel node, Point @from, Point to): GraphAction
        {
            public void Undo(Diagram diagram)
            {
                node.SetPosition(from.X, from.Y);
            }

            public void Redo(Diagram diagram)
            {
                node.SetPosition(to.X, to.Y);
            }

            public GraphAction WithTo(Point to)
            {
                return new MoveNode(node, from, to);
            }
        }

        record RemoveNode(NodeModel node): GraphAction
        {
            public void Undo(Diagram diagram)
            {
                diagram.Nodes.Add(node);
            }

            public void Redo(Diagram diagram)
            {
                diagram.Nodes.Remove(node);
            }
        }
        
        record AddLink(BaseLinkModel link): GraphAction
        {
            public void Undo(Diagram diagram)
            {
                diagram.Links.Remove(link);
            }

            public void Redo(Diagram diagram)
            {
                diagram.Links.Add(link);
            }
        }
        
        record RemoveLink(BaseLinkModel link): GraphAction
        {
            public void Undo(Diagram diagram)
            {
                diagram.Links.Add(link);
            }

            public void Redo(Diagram diagram)
            {
                diagram.Links.Remove(link);
            }
        }
    }
    
    private readonly Diagram diagram;
    private readonly List<GraphAction> undoActionList;
    private readonly List<GraphAction> redoActionList;
    private int actionHistoryMaxSize = 200;
    private bool isTrackingHistory, isDoing;
    private int checkpoint = 0;

    public DiagramHistory(Diagram diagram)
    {
        this.diagram = diagram;

        undoActionList = new();
        redoActionList = new();
        isTrackingHistory = true;

        diagram.KeyDown += KeyboardHandle;
        diagram.Links.Added += Links_Added;
        diagram.Links.Removed += Links_Removed;
        diagram.Nodes.Added += Nodes_Added;
        diagram.Nodes.Removed += Nodes_Removed;
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
        if (!undoActionList.Any()) return;

        isDoing = true;
        undoActionList[^1].Undo(diagram);
        RemoveLastUndoAction();
        diagram.UnselectAll();
        isDoing = false;
    }

    public void RedoLastAction()
    {
        if (!redoActionList.Any()) return;

        isDoing = true;
        redoActionList[^1].Redo(diagram);
        RemoveLastRedoAction();
        diagram.UnselectAll();
        isDoing = false;
    }

    private void RemoveLastUndoAction()
    {
        if (undoActionList.Any())
        {
            var action = undoActionList.Last();
            undoActionList.RemoveAt(undoActionList.Count - 1);
            redoActionList.Add(action);
        }
    }
    
    private void RemoveLastRedoAction()
    {
        if (redoActionList.Any())
        {
            var action = redoActionList.Last();
            redoActionList.RemoveAt(redoActionList.Count - 1);
            undoActionList.Add(action);
        }
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

       
    private void Link_Connected(BaseLinkModel arg1, PortModel _, PortModel outPort)
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

    private void Nodes_Removed(NodeModel node)
    {
        RegisterUndoHistoryAction(new GraphAction.RemoveNode(node));
    }

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
}