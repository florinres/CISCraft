using Ui.Models;

namespace Ui.ViewModels.Components.Diagram;

public partial class RegisterViewModel : BaseDiagramObject
{
    [ObservableProperty]
    public partial object Value { get; set; }
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty] public partial NumberFormat Format { get; set; } = NumberFormat.Binary;
    // public EventHandler customEvent;
    //
    // private void DoSmthAfterEvent(object? sender, EventArgs e)
    // {
    //     Value = e.ToString();
    // }

    public RegisterViewModel(string name, object? defaultValue = null)
    {
        Name = name;
        Value = defaultValue ?? 0;
    }

    public override string ToString()
    {
        switch (Format)
        {
            case NumberFormat.Binary:
                return Convert.ToString(0x0, 2).PadLeft(16, '0').Insert(4, " ").Insert(9, " ").Insert(14, " ");
            default:
                throw new NotImplementedException($"The format {Format} is not implemented");
        }
    }
}