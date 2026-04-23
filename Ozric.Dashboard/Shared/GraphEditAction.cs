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

    record AddNode(GraphNode GraphNode): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            if (editor.Graph.HasNode(GraphNode.id))
                throw new Exception($"Node with ID {GraphNode.id} already exists");

            editor.Graph.AddNode(GraphNode);
            var pos = editor.Diagram.GetScreenPoint(0.3, 0.2);
            var container = editor.Diagram.Container!;
            var normalized = LayoutCoordinateConverter.ToNormalized(pos, container);
            editor.GraphLayout.SetNodePosition(editor.AreaId, GraphNode.id, normalized);
            editor.AddNode(GraphNode, pos);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RemoveNode(GraphNode);
            editor.GraphLayout.RemoveNode(editor.AreaId, GraphNode.id);
        }
    }

    record RenameNode(GraphNode GraphNode, string NewId): GraphEditAction
    {
        private readonly string _oldId = GraphNode.id;
        
        public void Do(AreaGraphView editor)
        {
            editor.RenameNode(_oldId, NewId);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RenameNode(NewId, _oldId);
        }
    }

    record EditNode(GraphNode GraphNode, PropertyInfo Property, object? NewValue): GraphEditAction
    {
        private readonly object? _oldValue = Property.GetValue(GraphNode);
        
        public void Do(AreaGraphView editor)
        {
            Property.SetValue(GraphNode, NewValue);
            editor.Reload(GraphNode);
        }

        public void Undo(AreaGraphView editor)
        {
            Property.SetValue(GraphNode, _oldValue);
            editor.Reload(GraphNode);
        }
    }
    
    record AddInput(GraphVariableInputs Node, string InputName): GraphEditAction
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

    record RemoveInput(GraphVariableInputs Node, string InputName): GraphEditAction
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

    record RemoveNode(GraphNode GraphNode): GraphEditAction
    {
        private LayoutPoint? _savedNorm;

        public void Do(AreaGraphView editor)
        {
            _savedNorm = editor.GraphLayout.GetNodePosition(editor.AreaId, GraphNode.id);
            editor.RemoveNode(GraphNode);
            editor.GraphLayout.RemoveNode(editor.AreaId, GraphNode.id);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.Graph.AddNode(GraphNode);
            if (_savedNorm != null)
                editor.GraphLayout.SetNodePosition(editor.AreaId, GraphNode.id, _savedNorm);
            var container = editor.Diagram.Container!;
            var pixelPos = _savedNorm != null
                ? LayoutCoordinateConverter.ToPixels(_savedNorm, container)
                : Point.Zero;
            editor.AddNode(GraphNode, pixelPos);
        }
    }
    
    record AddEdge(GraphEdge GraphEdge): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            if (!editor.Graph.HasOutput(GraphEdge.from))
                throw new Exception();
            
            if (!editor.Graph.HasInput(GraphEdge.to))
                throw new Exception();
            
            editor.Graph.edges.Add(GraphEdge.id, GraphEdge);
            editor.AddEdge(GraphEdge);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.RemoveEdge(GraphEdge);
            editor.Graph.edges.Remove(GraphEdge.id);
        }
    }
    
    record RemoveEdge(GraphEdge GraphEdge): GraphEditAction
    {
        public void Do(AreaGraphView editor)
        {
            editor.RemoveEdge(GraphEdge);
            editor.Graph.edges.Remove(GraphEdge.id);
        }

        public void Undo(AreaGraphView editor)
        {
            editor.Graph.edges.Add(GraphEdge.id, GraphEdge);
            editor.AddEdge(GraphEdge);
        }
    }
}