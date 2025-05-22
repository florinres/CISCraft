using Ui.Views.UserControls.Diagram;

namespace Ui.ViewModels.Components.Diagram;

public class BitBlockViewModel : RegisterViewModel
{
    public BitBlockViewModel(string name, object? defaultValue = null) : base(name, defaultValue)
    {
        Value = defaultValue ?? 0;
    }
}