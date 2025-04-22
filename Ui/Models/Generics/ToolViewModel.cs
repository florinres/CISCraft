namespace Ui.Models.Generics;

public abstract partial class ToolViewModel : PaneViewModel
{
    [ObservableProperty]
    protected bool _isVisible = false;
}