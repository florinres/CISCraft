namespace Ui.ViewModels.Components.Diagram;

public partial class RegisterViewModel : BaseDiagramObject
{
    [ObservableProperty]
    private object _value;
    
    [ObservableProperty]
    private string _name;
    
    // public EventHandler customEvent;
    //
    // private void DoSmthAfterEvent(object? sender, EventArgs e)
    // {
    //     Value = e.ToString();
    // }

    public RegisterViewModel(string name, object? defaultValue = null)
    {
        Name = name;
        Value = defaultValue ?? "0000000000000000";
    }
    
}