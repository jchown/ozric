using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
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

        record MoveNode(NodeModel nodeModel, Point @from, Point to): GraphAction
        {
            public void Undo(Diagram diagram)
            {
                nodeModel.Position = from;
            }

            public void Redo(Diagram diagram)
            {
                nodeModel.Position = to;
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
    }
    
    private readonly Diagram diagram;
    private readonly List<GraphAction> undoActionList;
    private readonly List<GraphAction> redoActionList;
    private int actionHistoryMaxSize = 200;
    private bool isTrackingHistory, undoActionFlag, redoActionFlag;
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

        undoActionFlag = true;
        RevertAction(undoActionList[^1]);
        RemoveLastUndoAction();
        diagram.UnselectAll();
    }

    public void RedoLastAction()
    {
        if (!redoActionList.Any()) return;
        redoActionFlag = true;
        RevertAction(redoActionList[^1]);
        RemoveLastRedoAction();
        diagram.UnselectAll();
    }

    private void RevertAction(GraphAction action)
    {
        action.Undo(diagram);
    }

    private void RemoveLastUndoAction()
    {
        if (undoActionList.Any())
            undoActionList.RemoveAt(undoActionList.Count - 1);
    }

    private void ClearRedoList() => redoActionList.Clear();
    
    private void RegisterRedoHistoryAction(GraphAction action)
    {
        redoActionList.Add(action);
    }

    private void RemoveLastRedoAction()
    {
        if (redoActionList.Any())
            redoActionList.RemoveAt(redoActionList.Count - 1);
    }

    private void RegisterUndoHistoryAction(GraphAction action)
    {
        if (!isTrackingHistory) return;

        if (undoActionFlag && !redoActionFlag)
        {
            RegisterRedoHistoryAction(action);
            undoActionFlag = false;
            return;
        }
        
        if (redoActionFlag)
            redoActionFlag = false;
        else
            ClearRedoList();

        if (undoActionList.Count > actionHistoryMaxSize)
        {
            undoActionList.RemoveAt(0);
            checkpoint--;
        }

        undoActionList.Add(action);
    }

    private void Links_Added(Blazor.Diagrams.Core.Models.Base.BaseLinkModel link)
    {
        if (link.TargetNode is null)
            link.TargetPortChanged += Link_Connected; //In case its a empty link being dragged (listen for its connection)
        else
            //In case it was connected instantaneously (via code)
            RegisterUndoHistoryAction(new GraphAction.AddLink, link); 
    }

       
    private void Link_Connected(Blazor.Diagrams.Core.Models.Base.BaseLinkModel arg1, PortModel _, PortModel outPort)
    {
        arg1.SourcePortChanged -= Link_Connected;
        RegisterUndoHistoryAction(GraphAction.ActionType.AddLink, arg1);
    }

    private void Links_Removed(Blazor.Diagrams.Core.Models.Base.BaseLinkModel link)
    {
        if (link.IsAttached)
            RegisterUndoHistoryAction(GraphAction.ActionType.RemoveLink, link);
    }

    private void Nodes_Added(NodeModel obj)
    {
        RegisterUndoHistoryAction(new GraphAction.AddNode(obj));
    }

    private void Nodes_Removed(NodeModel obj)
    {
        RegisterUndoHistoryAction(new GraphAction.RemoveNode(obj));
    }

    public void Node_Moved(NodeModel obj, Point from, Point to)
    {
        RegisterUndoHistoryAction(new GraphAction.MoveNode(obj, from, to));
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