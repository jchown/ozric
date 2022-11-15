using System.Collections.ObjectModel;
using System.Reflection;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;
using OzricUI;
using OzricUI.Components;

/// <summary>
/// Any action that is modelled in the edit history and hence can be undone/redone. 
/// </summary>

public interface GraphEditAction
{
    void Undo(GraphEditor editor);
    void Do(GraphEditor editor);

    record AddNode(Node node): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.Graph.AddNode(node);
            var pos = editor.diagram.GetScreenPoint(0.3, 0.2);
            editor.GraphLayout.nodeLayout[node.id] = LayoutPoint.FromPoint(pos);
            editor.AddNode(node, pos);
        }
        
        public void Undo(GraphEditor editor)
        {
            editor.RemoveNode(node);
            editor.GraphLayout.nodeLayout.Remove(node.id);
            editor.Graph.RemoveNode(node);
        }
    }

    record RenameNode(Node node, string newID): GraphEditAction
    {
        private string oldID = node.id;
        
        public void Do(GraphEditor editor)
        {
            editor.RenameNode(oldID, newID);
        }

        public void Undo(GraphEditor editor)
        {
            editor.RenameNode(newID, oldID);
        }
    }

    record EditNode(Node node, PropertyInfo property, object? newValue): GraphEditAction
    {
        private object? oldValue = property.GetValue(node);
        
        public void Do(GraphEditor editor)
        {
            property.SetValue(node, newValue);
            editor.Reload(node);
        }

        public void Undo(GraphEditor editor)
        {
            property.SetValue(node, oldValue);
            editor.Reload(node);
        }
    }
    
    record AddInput(VariableInputs node, string inputName): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.AddNodeInput(node, inputName);
            editor.Reload(node);
        }

        public void Undo(GraphEditor editor)
        {
            editor.RemoveNodeInput(node, inputName);
            editor.Reload(node);
        }
    }

    record RemoveInput(VariableInputs node, string inputName): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.RemoveNodeInput(node, inputName);
            editor.Reload(node);
        }
        
        public void Undo(GraphEditor editor)
        {
            editor.AddNodeInput(node, inputName);
            editor.Reload(node);
        }
    }

    record MoveNode(NodeModel node, Point @from, Point to): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            node.SetPosition(to.X, to.Y);
        }

        public void Undo(GraphEditor editor)
        {
            node.SetPosition(from.X, from.Y);
        }
        
        public GraphEditAction WithTo(Point to)
        {
            return this with { to = to };
        }
    }

    record RemoveNode(Node node): GraphEditAction
    {
        private Point position = Point.Zero;

        public void Do(GraphEditor editor)
        {
            position = editor.GetPosition(node);
            editor.RemoveNode(node);
        }

        public void Undo(GraphEditor editor)
        {
            editor.Graph.AddNode(node);
            editor.AddNode(node, position);
        }
    }
    
    record AddEdge(Edge edge): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.Graph.edges.Add(edge.id, edge);
            editor.AddEdge(edge);
        }

        public void Undo(GraphEditor editor)
        {
            editor.RemoveEdge(edge);
            editor.Graph.edges.Remove(edge.id);
        }
    }
    
    record RemoveEdge(Edge edge): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.RemoveEdge(edge);
            editor.Graph.edges.Remove(edge.id);
        }

        public void Undo(GraphEditor editor)
        {
            editor.Graph.edges.Add(edge.id, edge);
            editor.AddEdge(edge);
        }
    }
            
    record EditActions(List<GraphEditAction> actions): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            foreach (var action in actions)
                action.Do(editor);
        }

        public void Undo(GraphEditor editor)
        {
            for (int i = actions.Count; --i >= 0;)
                actions[i].Undo(editor);
        }
    }

    record Group(List<String> nodeIDs): GraphEditAction
    {
        private string groupID;
        
        public void Do(GraphEditor editor)
        {
            var zone = editor.GraphLayout.AddZone(nodeIDs);
            groupID = editor.AddZone(zone);
        }

        public void Undo(GraphEditor editor)
        {
            var group = editor.diagram.Groups.First(g => g.Id == groupID);
            foreach (var child in group.Children.ToArray())
                group.RemoveChild(child);
            editor.diagram.RemoveGroup(group);
        }
    }

    public static readonly ReadOnlyCollection<GraphEditAction> NoChanges = new (new List<GraphEditAction>());
}