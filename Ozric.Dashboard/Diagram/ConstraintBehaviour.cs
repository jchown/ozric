using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Ozric.Dashboard.Model;

namespace Ozric.Dashboard.Diagram;

/// <summary>
/// A variant of the DragMovablesBehavior that constrains the movement of nodes.
/// </summary>
public class ConstraintBehavior : Behavior
{
    private readonly Dictionary<MovableModel, Point> _initialPositions;
    private double? _lastClientX;
    private double? _lastClientY;
    private bool _moved;

    public ConstraintBehavior(BlazorDiagram diagram) : base(diagram)
    {
        _initialPositions = new Dictionary<MovableModel, Point>();
        
        diagram.PointerDown += OnPointerDown;
        diagram.PointerMove += OnPointerMove;
        diagram.PointerUp += OnPointerUp;
    }

    private void OnPointerDown(Blazor.Diagrams.Core.Models.Base.Model? model, PointerEventArgs e)
    {
        if (model is not MovableModel)
            return;

        _initialPositions.Clear();
        foreach (var sm in Diagram.GetSelectedModels())
        {
            if (sm is not MovableModel movable || movable.Locked)
                continue;

            // Special case: groups without auto size on
            if (sm is NodeModel node && node.Group != null && !node.Group.AutoSize)
                continue;

            var (x, y) = ApplyConstraints(movable, movable.Position.X, movable.Position.Y);
                
            _initialPositions.Add(movable, new Point(x, y));
        }

        _lastClientX = e.ClientX;
        _lastClientY = e.ClientY;
        _moved = false;
    }

    private void OnPointerMove(Blazor.Diagrams.Core.Models.Base.Model? model, PointerEventArgs e)
    {
        if (_initialPositions.Count == 0 || _lastClientX == null || _lastClientY == null)
            return;

        _moved = true;
        var deltaX = (e.ClientX - _lastClientX.Value) / Diagram.Zoom;
        var deltaY = (e.ClientY - _lastClientY.Value) / Diagram.Zoom;

        foreach (var (movable, initialPosition) in _initialPositions)
        {
            var (ndx, ndy) = ApplyConstraints(movable, deltaX + initialPosition.X, deltaY + initialPosition.Y);
            
            movable.SetPosition(ndx, ndy);
        }
    }

    private void OnPointerUp(Blazor.Diagrams.Core.Models.Base.Model? model, PointerEventArgs e)
    {
        if (_initialPositions.Count == 0)
            return;

        if (_moved)
        {
            foreach (var (movable, _) in _initialPositions)
            {
                movable.TriggerMoved();
            }
        }
        
        _initialPositions.Clear();
        _lastClientX = null;
        _lastClientY = null;
    }

    private double ApplyGridSize(double n)
    {
        if (Diagram.Options.GridSize == null)
            return n;

        var gridSize = Diagram.Options.GridSize.Value;
        return gridSize * Math.Floor((n + gridSize / 2.0) / gridSize);
    }
    
    private (double, double) GridConstraints(Blazor.Diagrams.Core.Models.Base.Model model, double x, double y)
    {
        var ndx = ApplyGridSize(x);
        var ndy = ApplyGridSize(y);

        return (ndx, ndy);
    }

    private (double ndx, double ndy) ApplyConstraints(Blazor.Diagrams.Core.Models.Base.Model model, double x, double y)
    {
        return ApplyConstraints(Diagram, model, x, y);
    }
    
    public static (double ndx, double ndy) ApplyConstraints(Blazor.Diagrams.Core.Diagram diagram, Blazor.Diagrams.Core.Models.Base.Model model, double x, double y)
    {
        if (diagram.Container == null)
            return (x, y);
        
        var minX = diagram.Container.Left + 100;
        var maxX = diagram.Container.Right - 300;
        var minY = diagram.Container.Top ;
        var maxY = diagram.Container.Bottom - 300;
        
        if (model is IAreaSource)
        {
            maxX = minX;
        }
        
        if (model is IAreaSink)
        {
            minX = maxX;
        }
        
        return (Math.Clamp(x, minX, maxX), Math.Clamp(y, minY, maxY));
    }

    public override void Dispose()
    {
        _initialPositions.Clear();
        
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
    }
}
