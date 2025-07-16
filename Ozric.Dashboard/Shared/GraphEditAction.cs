using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;
using Ozric.Dashboard;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph;

/// <summary>
/// Any action that is modelled in the edit history and hence can be undone/redone. 
/// </summary>

public interface GraphEditAction
{
    void Do(AreaView editor);

    void Undo(AreaView editor);

    public static readonly ReadOnlyCollection<GraphEditAction> NoChanges = new (new List<GraphEditAction>());

    /// <summary>
    /// Generator interface for building composite actions
    /// </summary>
    public static GraphEditAction Build(IEnumerable<GraphEditAction> actionsGenerator)
    {
        return new EditActions(actionsGenerator.ToList());
    }

    /// <summary>
    /// Composite actions
    /// </summary>
    /// <param name="Actions"></param>
    record EditActions(List<GraphEditAction> Actions): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            foreach (var action in Actions)
                action.Do(editor);
        }

        public void Undo(AreaView editor)
        {
            for (int i = Actions.Count; --i >= 0;)
                Actions[i].Undo(editor);
        }
    }

    record AddNode(Node Node): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            if (editor.Graph.HasNode(Node.id))
                throw new Exception($"Node with ID {Node.id} already exists");

            editor.Graph.AddNode(Node);
            var pos = editor.Diagram.GetScreenPoint(0.3, 0.2);
            editor.GraphLayout.SetNodePosition(editor.AreaId, Node.id, LayoutPoint.FromPoint(pos));
            editor.AddNode(Node, pos);
        }
        
        public void Undo(AreaView editor)
        {
            editor.RemoveNode(Node);
            editor.GraphLayout.RemoveNode(editor.AreaId, Node.id);
        }
    }

    record RenameNode(Node Node, string NewId): GraphEditAction
    {
        private readonly string _oldId = Node.id;
        
        public void Do(AreaView editor)
        {
            editor.RenameNode(_oldId, NewId);
        }

        public void Undo(AreaView editor)
        {
            editor.RenameNode(NewId, _oldId);
        }
    }

    record EditNode(Node Node, PropertyInfo Property, object? NewValue): GraphEditAction
    {
        private readonly object? _oldValue = Property.GetValue(Node);
        
        public void Do(AreaView editor)
        {
            Property.SetValue(Node, NewValue);
            editor.Reload(Node);
        }

        public void Undo(AreaView editor)
        {
            Property.SetValue(Node, _oldValue);
            editor.Reload(Node);
        }
    }
    
    record AddInput(VariableInputs Node, string InputName): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            editor.AddNodeInput(Node, InputName);
            editor.Reload(Node);
        }

        public void Undo(AreaView editor)
        {
            editor.RemoveNodeInput(Node, InputName);
            editor.Reload(Node);
        }
    }

    record RemoveInput(VariableInputs Node, string InputName): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            editor.RemoveNodeInput(Node, InputName);
            editor.Reload(Node);
        }
        
        public void Undo(AreaView editor)
        {
            editor.AddNodeInput(Node, InputName);
            editor.Reload(Node);
        }
    }

    record MoveNode(NodeModel Node, Point From, Point To): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            Node.SetPosition(To.X, To.Y);
        }

        public void Undo(AreaView editor)
        {
            Node.SetPosition(From.X, From.Y);
        }
        
        public MoveNode WithTo(Point to)
        {
            return this with { To = to };
        }
    }

    record MoveNodes(List<MoveNode> Moves): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            foreach (var action in Moves)
                action.Do(editor);
        }

        public void Undo(AreaView editor)
        {
            for (int i = Moves.Count; --i >= 0;)
                Moves[i].Undo(editor);
        }
        
        public GraphEditAction WithTo(List<MoveNode> moves)
        {
            return this with { Moves = Moves.Zip(moves, (a,b) => a.WithTo(b.To)).ToList() };
        }
    }

    record RemoveNode(Node Node): GraphEditAction
    {
        private Point _position = Point.Zero;

        public void Do(AreaView editor)
        {
            _position = editor.GetPosition(Node);
            editor.RemoveNode(Node);
        }

        public void Undo(AreaView editor)
        {
            editor.Graph.AddNode(Node);
            editor.AddNode(Node, _position);
        }
    }
    
    record AddEdge(Edge Edge): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            if (!editor.Graph.HasOutput(Edge.from))
                throw new Exception();
            
            if (!editor.Graph.HasInput(Edge.to))
                throw new Exception();
            
            editor.Graph.edges.Add(Edge.id, Edge);
            editor.AddEdge(Edge);
        }

        public void Undo(AreaView editor)
        {
            editor.RemoveEdge(Edge);
            editor.Graph.edges.Remove(Edge.id);
        }
    }
    
    record RemoveEdge(Edge Edge): GraphEditAction
    {
        public void Do(AreaView editor)
        {
            editor.RemoveEdge(Edge);
            editor.Graph.edges.Remove(Edge.id);
        }

        public void Undo(AreaView editor)
        {
            editor.Graph.edges.Add(Edge.id, Edge);
            editor.AddEdge(Edge);
        }
    }
}