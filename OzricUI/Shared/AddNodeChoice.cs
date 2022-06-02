using OzricEngine;

namespace OzricUI.Shared;

public class AddNodeChoice
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public Action<Graph> Add { get; set; }

    public AddNodeChoice(string name, string icon, Action<Graph> add)
    {
        Name = name;
        Icon = icon;
        Add = add;
    }
}