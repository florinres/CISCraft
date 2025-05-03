using System.Collections.ObjectModel;

namespace Ui.ViewModels.Components.Diagram;

public partial class MicroprogramMemoryViewModel: ObservableObject
{
    [ObservableProperty] public partial ObservableCollection<string> MicroProgramItems { get; set; } =
    [
        "Push FLAG",
        "pd_ir[offset]",
        "PD FLAGS",
        "none",
        "PM_FLAG",
        "(CIN+PD_COND)",
        "WRITE",
        "IF B2 JUMP! (IR13,IR12,...)",
        "CLR ELSE JUMP! (IR14,...)",
        "MOV"
    ];
}