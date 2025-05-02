// ReSharper disable InconsistentNaming

namespace Ui.ViewModels.Generics;

public abstract partial class PaneViewModel : ObservableObject
{
    [ObservableProperty] public partial string? Title { get; set; } = null;

    [ObservableProperty] public partial string? ContentId { get; set; } = null;

    [ObservableProperty] public partial bool IsSelected { get; set; } = false;

    [ObservableProperty] public partial bool IsActive { get; set; } = true;
}