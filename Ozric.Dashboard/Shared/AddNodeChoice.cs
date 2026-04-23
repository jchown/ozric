using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;
using Ozric.Dashboard.Model;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Graph.Logic;
using Ozric.Engine.Nodes;
using BinaryChoice = Ozric.Dashboard.Model.BinaryChoice;
using BinarySustain = Ozric.Dashboard.Model.BinarySustain;
using Constant = OzricEngine.Nodes.Constant;
using DayPhases = Ozric.Dashboard.Model.DayPhases;
using IfAll = Ozric.Dashboard.Model.IfAll;
using IfAny = Ozric.Dashboard.Model.IfAny;
using Light = Ozric.Dashboard.Model.Light;
using MediaPlayer = Ozric.Dashboard.Model.MediaPlayer;
using ModeMatch = Ozric.Dashboard.Model.ModeMatch;
using ModeSensor = Ozric.Dashboard.Model.ModeSensor;
using ModeSwitch = Ozric.Dashboard.Model.ModeSwitch;
using NumberCompare = Ozric.Dashboard.Model.NumberCompare;
using Person = Ozric.Dashboard.Model.Person;
using SkyBrightness = Ozric.Dashboard.Model.SkyBrightness;
using Switch = Ozric.Dashboard.Model.Switch;
using Tween = Ozric.Dashboard.Model.Tween;
using ValueType = Ozric.Engine.Values.ValueType;
using Weather = Ozric.Dashboard.Model.Weather;

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
            icon: IfAny.ICON,
            create: () => new Engine.Graph.Logic.IfAny(graph.CreateNodeID("ifany"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If All - AND",
            icon: IfAll.ICON,
            create: () => new Engine.Graph.Logic.IfAll(graph.CreateNodeID("ifall"))));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Color",
            icon: ConstantColor.ICON,
            create: () => new Constant(graph.CreateNodeID("colour"), ColorRGB.WHITE)));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Number",
            icon: ConstantNumber.ICON,
            create: () => new Constant(graph.CreateNodeID("number"), new Number(0))));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Binary",
            icon: ConstantBinary.ICON,
            create: () => new Constant(graph.CreateNodeID("binary"), new Binary(false))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Compare Number",
            icon: NumberCompare.ICON,
            create: () => new Engine.Graph.Logic.NumberCompare(graph.CreateNodeID("compare"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Matches",
            icon: ModeMatch.ICON,
            create: () => new Engine.Graph.Logic.ModeMatch(graph.CreateNodeID("match"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Color",
            icon: Tween.ICON,
            create: () => new Engine.Graph.Logic.Tween(graph.CreateNodeID("tween"), ValueType.Color)));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Number",
            icon: Tween.ICON,
            create: () => new Engine.Graph.Logic.Tween(graph.CreateNodeID("tween"), ValueType.Number)));
        
        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Choice - Color",
            icon: BinaryChoice.ICON,
            create: () => BinaryChoice.Color(graph.CreateNodeID("binary-choice"))));
        
        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Sustain",
            icon: BinarySustain.ICON,
            create: () => new Engine.Graph.Logic.BinarySustain(graph.CreateNodeID("sustain"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Switch - Color",
            icon: ModeSwitch.ICON,
            create: () => ModeSwitch.Color(graph.CreateNodeID("color-mode-switch"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Day Phases",
            icon: DayPhases.ICON,
            create: () => new OzricEngine.Nodes.DayPhases(graph.CreateNodeID("day-phases"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Sky Brightness",
            icon: SkyBrightness.ICON,
            create: () => new OzricEngine.Nodes.SkyBrightness(graph.CreateNodeID("sky-brightness"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Weather",
            icon: Weather.ICON,
            create: () => new OzricEngine.Nodes.Weather(graph.CreateNodeID("weather"))));

        return choices;
    }

    internal static string? GetEntityIcon(Type type)
    {
        if (type == typeof(Engine.Graph.Entities.Light))
            return Light.ICON;

        if (type == typeof(BinarySensor))
            return Sensor.ICON;

        if (type == typeof(OzricEngine.Nodes.Person))
            return Person.ICON;

        if (type == typeof(OzricEngine.Nodes.ModeSensor))
            return ModeSensor.ICON;

        if (type == typeof(OzricEngine.Nodes.Switch))
            return Switch.ICON;

        if (type == typeof(OzricEngine.Nodes.MediaPlayer))
            return MediaPlayer.ICON;

        return null;
    }
}