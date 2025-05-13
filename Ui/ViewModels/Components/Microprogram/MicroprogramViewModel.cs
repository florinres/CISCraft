using System.Collections.ObjectModel;
using Ui.Models;
using Ui.ViewModels.Components.Diagram;

namespace Ui.ViewModels.Components.Microprogram;

public partial class MicroprogramViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<MicroprogramMemoryViewModel> Rows { get; set; } = [];

    [ObservableProperty] public partial NumberFormat AddressFormat { get; set; } = NumberFormat.Hex;
}