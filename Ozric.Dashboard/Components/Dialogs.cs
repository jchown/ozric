using MudBlazor;

namespace Ozric.Dashboard.Components;

public static class Dialogs
{
    public static DialogOptions StandardOptions()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            Position = DialogPosition.TopCenter,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        return options;
    }
}
