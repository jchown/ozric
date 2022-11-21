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
    void Do(GraphEditor editor);

    void Undo(GraphEditor editor);

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
        public void Do(GraphEditor editor)
        {
            foreach (var action in Actions)
                action.Do(editor);
        }

        public void Undo(GraphEditor editor)
        {
            for (int i = Actions.Count; --i >= 0;)
                Actions[i].Undo(editor);
        }
    }

    record AddNode(Node Node): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            if (editor.Graph.HasNode(Node.id))
                throw new Exception($"Node with ID {Node.id} already exists");

            editor.Graph.AddNode(Node);
            var pos = editor.diagram.GetScreenPoint(0.3, 0.2);
            editor.GraphLayout.nodeLayout[Node.id] = LayoutPoint.FromPoint(pos);
            editor.AddNode(Node, pos);
        }
        
        public void Undo(GraphEditor editor)
        {
            editor.RemoveNode(Node);
            editor.GraphLayout.nodeLayout.Remove(Node.id);
        }
    }

    record RenameNode(Node Node, string NewId): GraphEditAction
    {
        private readonly string _oldID = Node.id;
        
        public void Do(GraphEditor editor)
        {
            editor.RenameNode(_oldID, NewId);
        }

        public void Undo(GraphEditor editor)
        {
            editor.RenameNode(NewId, _oldID);
        }
    }

    record EditNode(Node Node, PropertyInfo Property, object? NewValue): GraphEditAction
    {
        private readonly object? _oldValue = Property.GetValue(Node);
        
        public void Do(GraphEditor editor)
        {
            Property.SetValue(Node, NewValue);
            editor.Reload(Node);
        }

        public void Undo(GraphEditor editor)
        {
            Property.SetValue(Node, _oldValue);
            editor.Reload(Node);
        }
    }
    
    record AddInput(VariableInputs Node, string InputName): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.AddNodeInput(Node, InputName);
            editor.Reload(Node);
        }

        public void Undo(GraphEditor editor)
        {
            editor.RemoveNodeInput(Node, InputName);
            editor.Reload(Node);
        }
    }

    record RemoveInput(VariableInputs Node, string InputName): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.RemoveNodeInput(Node, InputName);
            editor.Reload(Node);
        }
        
        public void Undo(GraphEditor editor)
        {
            editor.AddNodeInput(Node, InputName);
            editor.Reload(Node);
        }
    }

    record MoveNode(NodeModel Node, Point From, Point To): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            Node.SetPosition(To.X, To.Y);
        }

        public void Undo(GraphEditor editor)
        {
            Node.SetPosition(From.X, From.Y);
        }
        
        public GraphEditAction WithTo(Point to)
        {
            return this with { To = to };
        }
    }

    record RemoveNode(Node Node): GraphEditAction
    {
        private Point _position = Point.Zero;

        public void Do(GraphEditor editor)
        {
            _position = editor.GetPosition(Node);
            editor.RemoveNode(Node);
        }

        public void Undo(GraphEditor editor)
        {
            editor.Graph.AddNode(Node);
            editor.AddNode(Node, _position);
        }
    }
    
    record AddEdge(Edge Edge): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            if (!editor.Graph.HasOutput(Edge.from))
                throw new Exception();
            
            if (!editor.Graph.HasInput(Edge.to))
                throw new Exception();
            
            editor.Graph.edges.Add(Edge.id, Edge);
            editor.AddEdge(Edge);
        }

        public void Undo(GraphEditor editor)
        {
            editor.RemoveEdge(Edge);
            editor.Graph.edges.Remove(Edge.id);
        }
    }
    
    record RemoveEdge(Edge Edge): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            editor.RemoveEdge(Edge);
            editor.Graph.edges.Remove(Edge.id);
        }

        public void Undo(GraphEditor editor)
        {
            editor.Graph.edges.Add(Edge.id, Edge);
            editor.AddEdge(Edge);
        }
    }
            
    record AddGroup(string ZoneId): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            var zone = editor.GraphLayout.AddZone(ZoneId);
            editor.AddZone(zone);
        }

        public void Undo(GraphEditor editor)
        {
            var zone = editor.GetZone(ZoneId);
            editor.RemoveZone(zone);
            editor.GraphLayout.RemoveZone(ZoneId);
        }
    }

    record RemoveGroup(string ZoneId): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            var zone = editor.GetZone(ZoneId);
            editor.RemoveZone(zone);
            editor.GraphLayout.RemoveZone(ZoneId);
        }

        public void Undo(GraphEditor editor)
        {
            var zone = editor.GraphLayout.AddZone(ZoneId);
            editor.AddZone(zone);
        }
    }
    
    record AddNodesToGroup(string ZoneId, List<String> NodeIDs): GraphEditAction
    {
        public void Do(GraphEditor editor)
        {
            var group = editor.GetDiagramGroup(ZoneId);
            
            for (int i = 0; i < NodeIDs.Count; i++)
            {
                var nodeID = NodeIDs[i];
                var node = editor.GetDiagramNode(nodeID);
                if (node.Group != null)
                    throw new Exception($"Node {nodeID} is already in group {node.Group.Id}");
                
                group.AddChild(node);
                editor.GraphLayout.zones[ZoneId].nodeIDs.Add(nodeID);
            }
        }

        public void Undo(GraphEditor editor)
        {
            var group = editor.GetDiagramGroup(ZoneId);
            
            for (int i = 0; i < NodeIDs.Count; i++)
            {
                var nodeID = NodeIDs[i];
                var node = editor.GetDiagramNode(nodeID);
                
                editor.GraphLayout.zones[ZoneId].nodeIDs.Remove(nodeID);
                group.RemoveChild(node);
            }
        }
    }

    record RemoveNodesFromGroups(List<String> NodeIDs): GraphEditAction
    {
        private List<String?> _groupIDs;
        
        public void Do(GraphEditor editor)
        {
            _groupIDs = NodeIDs.Select(nodeID => editor.GetDiagramNode(nodeID).Group?.Id).ToList();
            
            for (int i = 0; i < NodeIDs.Count; i++)
            {
                var groupID = _groupIDs[i];
                if (groupID == null)
                    continue;

                var nodeID = NodeIDs[i];
                var node = editor.GetDiagramNode(nodeID);
                
                if (node.Group is ZoneModel group)
                {
                    var zoneID = group.zoneID;
                    editor.GraphLayout.zones[zoneID].nodeIDs.Remove(nodeID);
                    group.RemoveChild(node);
                }
            }
            
            editor.diagram.Refresh();
        }

        public void Undo(GraphEditor editor)
        {
            for (int i = 0; i < _groupIDs.Count; i++)
            {
                var groupID = _groupIDs[i];
                if (groupID == null)
                    continue;

                var nodeID = NodeIDs[i];
                var node = editor.GetDiagramNode(nodeID);
                
                editor.diagram.Groups.First(g => g.Id == groupID).AddChild(node);
                editor.GraphLayout.zones[groupID].nodeIDs.Add(nodeID);
            }
        }
    }
}