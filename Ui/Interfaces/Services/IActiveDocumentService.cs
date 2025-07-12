using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface IActiveDocumentService : INotifyPropertyChanged
{
    FileViewModel? SelectedDocument { get; set; }
    ObservableCollection<FileViewModel> Documents { get; }
    ObservableCollection<IToolViewModel> Tools { get; }
    FileStatsViewModel FileStats { get; set; }
    IDiagramViewModel Diagram { get; set; }
    IHexViewModel HexViewer { get; set; }
    IMicroprogramViewModel Microprogram { get; set; }
    event EventHandler? ActiveDocumentChanged;
    void ToggleToolVisibility(ToolViewModel tool);
    void SetDockingService(IDockingService dockingService);
}