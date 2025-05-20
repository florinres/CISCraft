using System.Collections.ObjectModel;
using Ui.Models;

namespace Ui.ViewModels.Components.Diagram;

public partial class MicroprogramMemoryViewModel : ObservableObject
{
    [ObservableProperty] public partial string Item0 {get; set;} = "";
    [ObservableProperty] public partial string Item1 {get; set;} = "";
    [ObservableProperty] public partial string Item2 {get; set;} = "";
    [ObservableProperty] public partial string Item3 {get; set;} = "";
    [ObservableProperty] public partial string Item4 {get; set;} = "";
    [ObservableProperty] public partial string Item5 {get; set;} = "";
    [ObservableProperty] public partial string Item6 {get; set;} = "";
    [ObservableProperty] public partial string Item7 {get; set;} = "";
    [ObservableProperty] public partial string Item8 {get; set;} = "";
    [ObservableProperty] public partial string Item9 {get; set;} = "";

    [ObservableProperty] public partial int Address { get; set; }

    public string GetFormattedAddress(NumberFormat format) => format switch
    {
        NumberFormat.Decimal => Address.ToString(),
        NumberFormat.Hex => $"0x{Address:X}",
        NumberFormat.Binary => "0b" + Convert.ToString(Address, 2).PadLeft(8, '0'),
        _ => Address.ToString()
    };
    
    public string this[int index]
    {
        get => index switch
        {
            0 => Item0,
            1 => Item1,
            2 => Item2,
            3 => Item3,
            4 => Item4,
            5 => Item5,
            6 => Item6,
            7 => Item7,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 7.")
        };
        set
        {
            switch (index)
            {
                case 0: Item0 = value; break;
                case 1: Item1 = value; break;
                case 2: Item2 = value; break;
                case 3: Item3 = value; break;
                case 4: Item4 = value; break;
                case 5: Item5 = value; break;
                case 6: Item6 = value; break;
                case 7: Item7 = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 7.");
            }
        }
    }
}