namespace OzricUI.Model;

[AttributeUsage(AttributeTargets.Class)]
public class EditDialogAttribute : Attribute
{
    public readonly Type type;
    public readonly string title;

    public EditDialogAttribute(Type type, string title)
    {
        this.type = type;
        this.title = title;
    }
}