using System.Reflection;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using OzricEngine.logic;
using OzricUI;
using OzricUI.Components;

public interface GraphAction
{
    void Undo(GraphEditor editor);
    void Do(GraphEditor editor);

    record AddNode(Node node): GraphAction
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

    record RenameNode(Node node, string newID): GraphAction
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

    record EditNode(Node node, PropertyInfo property, object? newValue): GraphAction
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
            editor.Graph.AddNode(node);
            editor.AddNode(node, position);
        }
    }
    
    record AddLink(BaseLinkModel link): GraphAction
    {
        public void Do(GraphEditor editor)
        {
            editor.diagram.Links.Add(link);
        }

        public void Undo(GraphEditor editor)
        {
            editor.diagram.Links.Remove(link);
        }
    }
    
    record RemoveLink(BaseLinkModel link): GraphAction
    {
        public void Do(GraphEditor editor)
        {
            editor.diagram.Links.Remove(link);
        }

        public void Undo(GraphEditor editor)
        {
            editor.diagram.Links.Add(link);
        }
    }
            
    record Actions(List<GraphAction> actions): GraphAction
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
}
