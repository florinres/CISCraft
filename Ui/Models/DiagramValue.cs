namespace Ui.Models;

public partial class DiagramValue : ObservableObject
{
    [ObservableProperty]
    public partial short Value { get; set; }
    [ObservableProperty]
    public partial NumberFormat Format { get; set; } = NumberFormat.Binary;

    public override string ToString()
    {
        return Format switch
        {
            NumberFormat.Binary => Convert.ToString(Value, 2).PadLeft(16, '0')
                .Insert(4, " ").Insert(9, " ").Insert(14, " "),
            NumberFormat.Hex => $"0x{Value:X4}",
            _ => Value.ToString()
        };
    }
}
