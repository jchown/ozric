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
                var category = device.Value.GetCategory();
                var type = CategoryModelMappings.Get(category);
                var entityID = device.Key;
                var id = entityID.Substring(entityID.IndexOf('.') + 1);
                if (graph.HasDevicesNode(id))
                    id = graph.CreateNodeID(category.ToString().ToLowerInvariant());
                
                return new AddNodeChoice(
                    category: category,
                    name: entityID,
                    icon: LightModel.ICON,
                    create: () => (Node) Activator.CreateInstance(type, id, entityID)!,
                    once: true);
            })
            .ToList();
        
        //  TODO: Do this via reflection?

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If Any - OR",
            icon: IfAnyModel.ICON,
            create: () => new IfAny(graph.CreateNodeID("ifany-"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "If All - AND",
            icon: IfAllModel.ICON,
            create: () => new IfAny(graph.CreateNodeID("ifall-"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Choice - Color",
            icon: BinaryChoiceModel.ICON,
            create: () => new BinaryChoice(graph.CreateNodeID("color-choice-"), ValueType.Color)));

        choices.Add(new AddNodeChoice(
            category: Category.Constant,
            name: "Color",
            icon: ConstantColorModel.ICON,
            create: () => new Constant(graph.CreateNodeID("colour-"), ColorRGB.WHITE)));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Matches",
            icon: ModeMatchModel.ICON,
            create: () => new ModeMatch(graph.CreateNodeID("match-"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Color",
            icon: TweenModel.ICON,
            create: () => new Tween(graph.CreateNodeID("tween-"), ValueType.Color)));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Tween - Scalar",
            icon: TweenModel.ICON,
            create: () => new Tween(graph.CreateNodeID("tween-"), ValueType.Scalar)));
        
        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Choice - Color",
            icon: BinaryChoiceModel.ICON,
            create: () => BinaryChoiceModel.Color(graph.CreateNodeID("binary-choice-"))));
        
        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Binary Sustain",
            icon: BinarySustainModel.ICON,
            create: () => new BinarySustain(graph.CreateNodeID("sustain-"))));

        choices.Add(new AddNodeChoice(
            category: Category.Logic,
            name: "Mode Switch - Color",
            icon: ModeSwitchModel.ICON,
            create: () => ModeSwitchModel.Color(graph.CreateNodeID("color-mode-switch-"))));

        choices.Add(new AddNodeChoice(
            category: Category.Environment,
            name: "Day Phases",
            icon: DayPhasesModel.ICON,
            create: () => new DayPhases(graph.CreateNodeID("day-phases-"))));

        return choices;
    }
}