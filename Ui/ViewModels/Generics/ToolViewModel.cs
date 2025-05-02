// ReSharper disable InconsistentNaming
namespace Ui.ViewModels.Generics;

public abstract partial class ToolViewModel : PaneViewModel
{
    [ObservableProperty]
    public partial bool IsVisible { get; set; } = false;
    
    public bool HasToolBeenLoaded { get; set; } = false;
}