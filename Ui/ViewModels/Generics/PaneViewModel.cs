// ReSharper disable InconsistentNaming

namespace Ui.ViewModels.Generics;

public abstract partial class PaneViewModel : ObservableObject
{
    public abstract  string? Title { get; set; }

    [ObservableProperty] public partial string? ContentId { get; set; } = null;

    [ObservableProperty] public partial bool IsSelected { get; set; } = false;

    [ObservableProperty] public partial bool IsActive { get; set; } = true;
}