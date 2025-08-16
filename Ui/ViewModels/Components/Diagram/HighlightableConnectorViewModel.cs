namespace Ui.ViewModels.Components.Diagram;

public partial class HighlightableConnectorViewModel : BaseDiagramObject
{
    public HighlightableConnectorViewModel(string name)
    {
        Name = name;
    }

    [ObservableProperty]
    public partial string Name { get; set; }
}