using System.ComponentModel;
using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph;

namespace Ozric.Dashboard.Model;

public static class DiagramTypeCatalogue
{
    private static readonly List<Type> DiagramTypes;

    private static readonly IDictionary<Type, Type> DiagramTypesForGraphTypes;

    static DiagramTypeCatalogue()
    {
        DiagramTypes = typeof(DiagramNode).Assembly.ExportedTypes
            .Where(t => typeof(DiagramNode).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();
        
        DiagramTypesForGraphTypes = new Dictionary<Type, Type>();
    }

    public static List<Type> GetDiagramTypes()
    {
        return DiagramTypes;
    }

    public static Type GetDiagramTypeFor(GraphNode graphNode)
    {
        var nodeType = graphNode.GetType();
        
        if (DiagramTypesForGraphTypes.TryGetValue(nodeType, out var type))
            return type;
        
        switch (graphNode)
        {
            // Special case for constants, as we want a specific type/UI for each value type

            case Ozric.Engine.Nodes.GraphConstant c:
            {
                switch (c.value.ValueType)
                {
                    case Engine.Values.ValueType.Color:
                        return typeof(DiagramConstantColor);

                    case Engine.Values.ValueType.Number:
                        return typeof(DiagramConstantNumber);

                    case Engine.Values.ValueType.Binary:
                        return typeof(DiagramConstantBinary);

                    case Engine.Values.ValueType.Mode:
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }

            default:
            {
                var constructors = GetDiagramTypes().SelectMany(t => t.GetConstructors());
                var applicable = constructors.First(c =>
                {
                    var p = c.GetParameters();
                    return (p.Length == 2 && p[0].ParameterType == nodeType &&
                            p[1].ParameterType == typeof(Point));
                });

                DiagramTypesForGraphTypes[nodeType] = applicable.DeclaringType!;
                break;
            }
        }

        return DiagramTypesForGraphTypes[nodeType];
    }
}