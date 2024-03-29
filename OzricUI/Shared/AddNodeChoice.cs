using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;
using OzricUI.Model;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Shared;

public class AddNodeChoice
{
    public readonly Category Category;
    public readonly string Name;
    public readonly string Icon;
    public readonly Func<Node> Create;
    public readonly bool Once;

    public AddNodeChoice(Category category, string name, string icon, Func<Node> create, bool once = false)
    {
        Category = category;
        Name = name;
        Icon = icon;
        Create = create;
        Once = once;
    }

    public static List<AddNodeChoice> GetChoices(Home home, Graph graph)
    {
        var choices = home.states
            .Where(device => !graph.HasDevicesNode(device.Key) && CategoryModelMappings.Exists(device.Value.GetCategory()))
            .Select(device =>
            {
                var entityID = device.Key;
                var category = device.Value.GetCategory();
                var type = CategoryModelMappings.Get(category);
                var icon = GetEntityIcon(type) ?? "icon-park-outline:chip";
                var id = entityID.Substring(entityID.IndexOf('.') + 1);
                if (graph.HasDevicesNode(id))
                    id = graph.CreateNodeID(category.ToString().ToLowerInvariant());
                
                return new AddNodeChoice(
                    category: category,
                    name: entityID,
                    icon: icon,
                    create: () => (Node) Activator.CreateInstance(type, id, entityID)!,
                    once: true);
            })
            .OrderBy(choice => choice.Category)
            .ToList();
        
        //  TODO: Do this via reflection?

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If Any - OR",
            icon: IfAnyModel.ICON,
            create: () => new IfAny(graph.CreateNodeID("ifany"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If All - AND",
            icon: IfAllModel.ICON,
            create: () => new IfAll(graph.CreateNodeID("ifall"))));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Color",
            icon: ConstantColorModel.ICON,
            create: () => new Constant(graph.CreateNodeID("colour"), ColorRGB.WHITE)));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Number",
            icon: ConstantNumberModel.ICON,
            create: () => new Constant(graph.CreateNodeID("number"), new Number(0))));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Binary",
            icon: ConstantBinaryModel.ICON,
            create: () => new Constant(graph.CreateNodeID("binary"), new Binary(false))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Compare Number",
            icon: NumberCompareModel.ICON,
            create: () => new NumberCompare(graph.CreateNodeID("compare"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Matches",
            icon: ModeMatchModel.ICON,
            create: () => new ModeMatch(graph.CreateNodeID("match"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Color",
            icon: TweenModel.ICON,
            create: () => new Tween(graph.CreateNodeID("tween"), ValueType.Color)));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Number",
            icon: TweenModel.ICON,
            create: () => new Tween(graph.CreateNodeID("tween"), ValueType.Number)));
        
        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Choice - Color",
            icon: BinaryChoiceModel.ICON,
            create: () => BinaryChoiceModel.Color(graph.CreateNodeID("binary-choice"))));
        
        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Sustain",
            icon: BinarySustainModel.ICON,
            create: () => new BinarySustain(graph.CreateNodeID("sustain"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Switch - Color",
            icon: ModeSwitchModel.ICON,
            create: () => ModeSwitchModel.Color(graph.CreateNodeID("color-mode-switch"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Day Phases",
            icon: DayPhasesModel.ICON,
            create: () => new DayPhases(graph.CreateNodeID("day-phases"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Sky Brightness",
            icon: SkyBrightnessModel.ICON,
            create: () => new SkyBrightness(graph.CreateNodeID("sky-brightness"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Weather",
            icon: WeatherModel.ICON,
            create: () => new Weather(graph.CreateNodeID("weather"))));

        return choices;
    }

    private static string? GetEntityIcon(Type type)
    {
        if (type == typeof(Light))
            return LightModel.ICON;

        if (type == typeof(BinarySensor))
            return SensorModel.ICON;

        if (type == typeof(Person))
            return PersonModel.ICON;

        if (type == typeof(ModeSensor))
            return ModeSensorModel.ICON;

        if (type == typeof(Switch))
            return SwitchModel.ICON;

        if (type == typeof(MediaPlayer))
            return MediaPlayerModel.ICON;

        return null;
    }
}