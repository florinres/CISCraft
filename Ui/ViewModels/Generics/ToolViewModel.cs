// ReSharper disable InconsistentNaming

using System.ComponentModel;
using Ui.Interfaces.ViewModel;

namespace Ui.ViewModels.Generics;

public abstract partial class ToolViewModel : PaneViewModel, IToolViewModel
{
    [ObservableProperty] public partial bool IsVisible { get; set; } = false;

    [ObservableProperty] public partial double ZoomFactor { get; set; }

    public bool HasToolBeenLoaded { get; set; } = false;
}