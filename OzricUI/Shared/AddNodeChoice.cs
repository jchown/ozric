using OzricEngine;
using OzricEngine.logic;

namespace OzricUI.Shared;

public class AddNodeChoice
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public Func<Node> Create { get; set; }

    public AddNodeChoice(string name, string icon, Func<Node> create)
    {
        Name = name;
        Icon = icon;
        Create = create;
    }
}