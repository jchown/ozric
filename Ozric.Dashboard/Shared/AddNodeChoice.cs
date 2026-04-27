using Ozric.Engine;
using Ozric.Engine.Nodes;
using Ozric.Engine.Values;
using Ozric.Dashboard.Model;
using Ozric.Dashboard.Models;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Graph.Logic;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Shared;

public class AddNodeChoice
{
    public readonly Category Category;
    public readonly string Name;
    public readonly string Icon;
    public readonly Func<GraphNode> Create;
    public readonly bool Once;

    public AddNodeChoice(Category category, string name, string icon, Func<GraphNode> create, bool once = false)
    {
        Category = category;
        Name = name;
        Icon = icon;
        Create = create;
        Once = once;
    }

    public static List<AddNodeChoice> GetChoices(IHome home, Graph graph, string areaId)
    {
        var entitiesInArea = home.GetEntitiesInArea(areaId);
        var entityStates = home.GetEntityStates();
        var usableDevices = entityStates.Where(device => CategoryModelMappings.Exists(device.GetCategory())).ToList();
        var unusedDevices = usableDevices.Where(device => !graph.HasEntityNode(device.entity_id)).ToList();
        var unusedDevicesInArea = unusedDevices.Where(device => entitiesInArea.Any(entity => entity.entity_id == device.entity_id)).ToList();
        var choices = unusedDevicesInArea
            .Select(device =>
            {
                var entityID = device.entity_id;
                var category = device.GetCategory();
                var type = CategoryModelMappings.Get(category);
                var icon = GetEntityIcon(type) ?? "icon-park-outline:chip";
                var id = entityID.Substring(entityID.IndexOf('.') + 1);
                if (graph.HasEntityNode(id))
                    id = graph.CreateNodeID(category.ToString().ToLowerInvariant());
                
                return new AddNodeChoice(
                    category: category,
                    name: entityID,
                    icon: icon,
                    create: () => (GraphNode) Activator.CreateInstance(type, id, entityID)!,
                    once: true);
            })
            .OrderBy(choice => choice.Category)
            .ToList();
        
        //  TODO: Do this via reflection?

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If Any - OR",
            icon: DiagramIfAny.ICON,
            create: () => new GraphIfAny(graph.CreateNodeID("ifany"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If All - AND",
            icon: DiagramIfAll.ICON,
            create: () => new GraphIfAll(graph.CreateNodeID("ifall"))));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Color",
            icon: DiagramConstantColor.ICON,
            create: () => new GraphConstant(graph.CreateNodeID("colour"), ColorRGB.WHITE)));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Number",
            icon: DiagramConstantNumber.ICON,
            create: () => new GraphConstant(graph.CreateNodeID("number"), new Number(0))));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Binary",
            icon: DiagramConstantBinary.ICON,
            create: () => new GraphConstant(graph.CreateNodeID("binary"), new Binary(false))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Compare Number",
            icon: DiagramNumberCompare.ICON,
            create: () => new GraphNumberCompare(graph.CreateNodeID("compare"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Matches",
            icon: DiagramModeMatch.ICON,
            create: () => new GraphModeMatch(graph.CreateNodeID("match"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Color",
            icon: DiagramTween.ICON,
            create: () => new GraphTween(graph.CreateNodeID("tween"), ValueType.Color)));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Number",
            icon: DiagramTween.ICON,
            create: () => new GraphTween(graph.CreateNodeID("tween"), ValueType.Number)));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Choice - Color",
            icon: DiagramBinaryChoice.ICON,
            create: () => DiagramBinaryChoice.Color(graph.CreateNodeID("binary-choice"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Sustain",
            icon: DiagramBinarySustain.ICON,
            create: () => new GraphBinarySustain(graph.CreateNodeID("sustain"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Switch - Color",
            icon: DiagramModeSwitch.ICON,
            create: () => DiagramModeSwitch.Color(graph.CreateNodeID("color-mode-switch"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Day Phases",
            icon: DiagramDayPhases.ICON,
            create: () => new GraphDayPhases(graph.CreateNodeID("day-phases"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Sky Brightness",
            icon: DiagramSkyBrightness.ICON,
            create: () => new GraphSkyBrightness(graph.CreateNodeID("sky-brightness"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Weather",
            icon: DiagramWeather.ICON,
            create: () => new GraphWeather(graph.CreateNodeID("weather"))));

        return choices;
    }

    internal static string? GetEntityIcon(Type type)
    {
        if (type == typeof(GraphLight))
            return DiagramLight.ICON;

        if (type == typeof(GraphBinarySensor))
            return DiagramSensor.ICON;

        if (type == typeof(Ozric.Engine.Nodes.GraphPerson))
            return DiagramPerson.ICON;

        if (type == typeof(Ozric.Engine.Nodes.GraphModeSensor))
            return DiagramModeSensor.ICON;

        if (type == typeof(Ozric.Engine.Nodes.GraphSwitch))
            return DiagramSwitch.ICON;

        if (type == typeof(Ozric.Engine.Nodes.GraphMediaPlayer))
            return DiagramMediaPlayer.ICON;

        return null;
    }
}