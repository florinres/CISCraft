using System.Collections.ObjectModel;
using Ui.Models;
using Ui.ViewModels.Components.Microprogram;

namespace Ui.ViewModels.Components.Diagram;

public enum HighlightType
{
    None,
    StepMicroCode,
    GoToRow
}
public partial class MicroprogramMemoryViewModel : ObservableObject
{
    public ObservableCollection<MicroInstructionItem> Items { get; set; } = [];

    public HighlightType HighlightType { get; set; }
    public bool IsCurrentStep { get; set; }
    [ObservableProperty] 
    public partial bool IsGoToTarget { get; set; } = false;

    [ObservableProperty] public partial int Address { get; set; }
    [ObservableProperty] public partial string Tag { get; set; } = "";
    [ObservableProperty] public partial bool IsCurrent { get; set; } = false;
    [ObservableProperty] public partial double HighlightOpacity { get; set; } = 1.0;

    public string GetFormattedAddress(NumberFormat format) => format switch
    {
        NumberFormat.Decimal => Address.ToString(),
        NumberFormat.Hex => $"0x{Address:X}",
        NumberFormat.Binary => "0b" + Convert.ToString(Address, 2).PadLeft(8, '0'),
        _ => Address.ToString()
    };
    
    public MicroInstructionItem this[int index]
    {
        get => Items[index];
        set
        {
            Items[index] = value;
        }
    }
}