namespace Ui.ViewModels.Components.Diagram;

public abstract partial class BaseDiagramObject : ObservableObject
{
    [ObservableProperty]
    private bool _isHighlighted = false;
    
    [ObservableProperty]
    private string _name = string.Empty;
}