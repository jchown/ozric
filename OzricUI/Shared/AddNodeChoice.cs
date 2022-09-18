using OzricEngine.Nodes;

namespace OzricUI.Shared;

public class AddNodeChoice
{
    public readonly Category Category;
    public readonly string Name;
    public readonly string Icon;
    public readonly Func<Node> Create;
    public readonly bool Once;

    public AddNodeChoice(Category category, string name, string icon, Func<Node> create, bool once)
    {
        Category = category;
        Name = name;
        Icon = icon;
        Create = create;
        Once = once;
    }
}