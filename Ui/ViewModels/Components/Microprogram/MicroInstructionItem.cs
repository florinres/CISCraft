namespace Ui.ViewModels.Components.Microprogram;

public class MicroInstructionItem
{
    public string Value { get; set; } = "";
    public bool IsCurrent { get; set; }

    public override string ToString()
    {
        return Value;
    }
}