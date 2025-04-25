// ReSharper disable InconsistentNaming
namespace Ui.ViewModels.Generics;

public abstract partial class ToolViewModel : PaneViewModel
{
    [ObservableProperty]
    protected bool _isVisible = false;
}