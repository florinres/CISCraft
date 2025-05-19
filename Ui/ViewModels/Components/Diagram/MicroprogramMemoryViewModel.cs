using System.Collections.ObjectModel;

namespace Ui.ViewModels.Components.Diagram;

public partial class MicroprogramMemoryViewModel : ObservableObject
{
    [ObservableProperty] public partial string Item0 {get; set;} = "Push FLAG";
    [ObservableProperty] public partial string Item1 {get; set;} = "pd_ir[offset]";
    [ObservableProperty] public partial string Item2 {get; set;} = "PD FLAGS";
    [ObservableProperty] public partial string Item3 {get; set;} = "none";
    [ObservableProperty] public partial string Item4 {get; set;} = "PM_FLAG";
    [ObservableProperty] public partial string Item5 {get; set;} = "(CIN+PD_COND)";
    [ObservableProperty] public partial string Item6 {get; set;} = "WRITE";
    [ObservableProperty] public partial string Item7 {get; set;} = "IF B2 JUMP! (IR13,IR12,IR11,IR10,IR9,IR8,IR7,IR6) CLR ELSE JUMP! (IR14,IR13,IE12,0) MOV";

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