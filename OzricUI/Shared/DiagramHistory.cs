using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Microsoft.AspNetCore.Components.Web;

namespace OzricUI.Shared;

public class DiagramHistory
{
    public class GraphAction
    {
        public enum ActionType { AddNode, RemoveNode, AddLink, RemoveLink }

        public ActionType actionType;

        public object actionData;

    }
    
    private readonly Diagram diagram;
    private readonly List<GraphAction> undoActionList;
    private readonly List<GraphAction> redoActionList;
    private int actionHistoryMaxSize = 200;
    private bool isTrackingHistory, undoActionFlag, redoActionFlag;

    public DiagramHistory(Diagram diagram)
    {
        this.diagram = diagram;

        undoActionList = new();
        redoActionList = new();
        isTrackingHistory = true;

        diagram.KeyDown += KeyboardHandle; //This should be on Init too
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
        switch (action.actionType)
        {
            case GraphAction.ActionType.AddNode:
                diagram.Nodes.Remove((NodeModel)action.actionData);
                break;
            case GraphAction.ActionType.RemoveNode:
                diagram.Nodes.Add((NodeModel)action.actionData);
                break;
            case GraphAction.ActionType.AddLink:
                diagram.Links.Remove((LinkModel)action.actionData);
                break;
            case GraphAction.ActionType.RemoveLink:
                diagram.Links.Add((LinkModel)action.actionData);
                break;
        }
    }

    private void RemoveLastUndoAction()
    {
        if (undoActionList.Any())
            undoActionList.RemoveAt(undoActionList.Count - 1);
    }

    private void ClearRedoList() => redoActionList.Clear();
    private void RegisterRedoHistoryAction(GraphAction.ActionType _actionType, object _data)
    {
        var action = new GraphAction { actionType = _actionType, actionData = _data };
        redoActionList.Add(action);
    }

    private void RemoveLastRedoAction()
    {
        if (redoActionList.Any())
            redoActionList.RemoveAt(redoActionList.Count - 1);
    }

    private void RegisterUndoHistoryAction(GraphAction.ActionType _actionType, object _data)
    {
        if (!isTrackingHistory) return;

        if (undoActionFlag && !redoActionFlag)
        {
            RegisterRedoHistoryAction(_actionType, _data);
            undoActionFlag = false;
            return;
        }
        if (redoActionFlag)
            redoActionFlag = false;
        else
            ClearRedoList();

        var action = new GraphAction { actionType = _actionType, actionData = _data };
        if (undoActionList.Count > actionHistoryMaxSize)
            undoActionList.RemoveAt(0);

        undoActionList.Add(action);
    }
    
    private void EventsSubscription()
    {
        diagram.Links.Added += Links_Added;
        diagram.Links.Removed += Links_Removed;
        diagram.Nodes.Added += Nodes_Added;
        diagram.Nodes.Removed += Nodes_Removed;
    }

    private void Links_Added(Blazor.Diagrams.Core.Models.Base.BaseLinkModel link)
    {
        if (link.TargetNode is null)
            link.TargetPortChanged += Link_Connected; //In case its a empty link being dragged (listen for its connection)
        else
            //In case it was connected instantaneously (via code)
            RegisterUndoHistoryAction(GraphAction.ActionType.AddLink, link); 
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
        RegisterUndoHistoryAction(GraphAction.ActionType.AddNode, obj);
    }


    private void Nodes_Removed(NodeModel obj)
    {
        RegisterUndoHistoryAction(GraphAction.ActionType.RemoveNode, obj);
    }
}