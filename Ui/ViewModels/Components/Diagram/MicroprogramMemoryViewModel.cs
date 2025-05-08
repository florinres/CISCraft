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
        "IF B2 JUMP! (IR13,IR12,IR11,IR10,IR9,IR8,IR7,IR6) CLR ELSE JUMP! (IR14,IR13,IE12,0) MOV"
    ];
}