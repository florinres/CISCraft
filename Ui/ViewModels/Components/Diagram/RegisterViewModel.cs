namespace Ui.ViewModels.Components.Diagram;

public partial class RegisterViewModel : BaseDiagramObject
{
    [ObservableProperty]
    public partial object Value { get; set; }
    [ObservableProperty]
    public partial string Name { get; set; }

    // public EventHandler customEvent;
    //
    // private void DoSmthAfterEvent(object? sender, EventArgs e)
    // {
    //     Value = e.ToString();
    // }

    public RegisterViewModel(string name, object? defaultValue = null)
    {
        Name = name;
        Value = defaultValue ?? "0000 0000 0000 0000";
    }
    
}