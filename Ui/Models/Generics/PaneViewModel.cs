namespace Ui.Models.Generics;

public abstract partial class PaneViewModel : ObservableObject
{
    [ObservableProperty]
    protected string? _title = null;
    [ObservableProperty]
    protected string? _contentId = null;
    [ObservableProperty]
    protected bool _isSelected = false;
    [ObservableProperty]
    protected bool _isActive = false;
}