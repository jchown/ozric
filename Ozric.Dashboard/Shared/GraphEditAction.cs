using System.Collections.ObjectModel;
using System.Reflection;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;
using Ozric.Dashboard;
using Ozric.Dashboard.Components;
using Ozric.Dashboard.Shared;
using Ozric.Engine.Graph;

/// <summary>
/// Any action that is modelled in the edit history and hence can be undone/redone. 
/// </summary>

public interface GraphEditAction
{
    void Do(AreaGraphView editor);

    void Undo(AreaGraphView editor);

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
        public void Do(AreaGraphView editor)
        {
            foreach (var action in Actions)
                action.Do(editor);
        }

        public void Undo(AreaGraphView editor)
        {
            for (int i = Actions.Count; --i >= 0;)
                Actions[i].Undo(editor);
        }
    }

    record AddNode(Node Node): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            if (editor.Graph.HasNode(Node.id))
                throw new Exception($"Node with ID {Node.id} already exists");

            editor.Graph.AddNode(Node);
            var pos = editor.Diagram.GetScreenPoint(0.3, 0.2);
            var container = editor.Diagram.Container!;
            var normalized = LayoutCoordinateConverter.ToNormalized(pos, container);
            editor.GraphLayout.SetNodePosition(editor.AreaId, Node.id, normalized);
            editor.AddNode(Node, pos);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RemoveNode(Node);
            editor.GraphLayout.RemoveNode(editor.AreaId, Node.id);
        }
    }

    record RenameNode(Node Node, string NewId): GraphEditAction
    {
        private readonly string _oldId = Node.id;
        
        public void Do(AreaGraphView editor)
        {
            editor.RenameNode(_oldId, NewId);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RenameNode(NewId, _oldId);
        }
    }

    record EditNode(Node Node, PropertyInfo Property, object? NewValue): GraphEditAction
    {
        private readonly object? _oldValue = Property.GetValue(Node);
        
        public void Do(AreaGraphView editor)
        {
            Property.SetValue(Node, NewValue);
            editor.Reload(Node);
        }

        public void Undo(AreaGraphView editor)
        {
            Property.SetValue(Node, _oldValue);
            editor.Reload(Node);
        }
    }
    
    record AddInput(VariableInputs Node, string InputName): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            editor.AddNodeInput(Node, InputName);
            editor.Reload(Node);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RemoveNodeInput(Node, InputName);
            editor.Reload(Node);
        }
    }

    record RemoveInput(VariableInputs Node, string InputName): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            editor.RemoveNodeInput(Node, InputName);
            editor.Reload(Node);
        }
        
        public void Undo(AreaGraphView editor)
        {
            editor.AddNodeInput(Node, InputName);
            editor.Reload(Node);
        }
    }

    record MoveNode(NodeModel Node, string NodeId, LayoutPoint FromNorm, LayoutPoint ToNorm): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            var container = editor.Diagram.Container!;
            var pixelPos = LayoutCoordinateConverter.ToPixels(ToNorm, container);
            Node.SetPosition(pixelPos.X, pixelPos.Y);
            editor.GraphLayout.SetNodePosition(editor.AreaId, NodeId, ToNorm);
        }

        public void Undo(AreaGraphView editor)
        {
            var container = editor.Diagram.Container!;
            var pixelPos = LayoutCoordinateConverter.ToPixels(FromNorm, container);
            Node.SetPosition(pixelPos.X, pixelPos.Y);
            editor.GraphLayout.SetNodePosition(editor.AreaId, NodeId, FromNorm);
        }

        public MoveNode WithTo(LayoutPoint to)
        {
            return this with { ToNorm = to };
        }
    }

    record MoveNodes(List<MoveNode> Moves): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            foreach (var action in Moves)
                action.Do(editor);
        }

        public void Undo(AreaGraphView editor)
        {
            for (int i = Moves.Count; --i >= 0;)
                Moves[i].Undo(editor);
        }

        public GraphEditAction WithTo(List<MoveNode> moves)
        {
            return this with { Moves = Moves.Zip(moves, (a, b) => a.WithTo(b.ToNorm)).ToList() };
        }
    }

    record RemoveNode(Node Node): GraphEditAction
    {
        private LayoutPoint? _savedNorm;

        public void Do(AreaGraphView editor)
        {
            _savedNorm = editor.GraphLayout.GetNodePosition(editor.AreaId, Node.id);
            editor.RemoveNode(Node);
            editor.GraphLayout.RemoveNode(editor.AreaId, Node.id);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.Graph.AddNode(Node);
            if (_savedNorm != null)
                editor.GraphLayout.SetNodePosition(editor.AreaId, Node.id, _savedNorm);
            var container = editor.Diagram.Container!;
            var pixelPos = _savedNorm != null
                ? LayoutCoordinateConverter.ToPixels(_savedNorm, container)
                : Point.Zero;
            editor.AddNode(Node, pixelPos);
        }
    }
    
    record AddEdge(Edge Edge): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            if (!editor.Graph.HasOutput(Edge.from))
                throw new Exception();
            
            if (!editor.Graph.HasInput(Edge.to))
                throw new Exception();
            
            editor.Graph.edges.Add(Edge.id, Edge);
            editor.AddEdge(Edge);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RemoveEdge(Edge);
            editor.Graph.edges.Remove(Edge.id);
        }
    }
    
    record RemoveEdge(Edge Edge): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            editor.RemoveEdge(Edge);
            editor.Graph.edges.Remove(Edge.id);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.Graph.edges.Add(Edge.id, Edge);
            editor.AddEdge(Edge);
        }
    }
}