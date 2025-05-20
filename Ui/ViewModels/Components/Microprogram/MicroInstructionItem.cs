namespace Ui.ViewModels.Components.Microprogram;

public partial class MicroInstructionItem : ObservableObject
{
    [ObservableProperty] public partial string Value { get; set; } = "";
    [ObservableProperty] public partial bool IsCurrent { get; set; }

    public override string ToString()
    {
        return Value;
    }
}