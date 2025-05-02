using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Services;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface IActiveDocumentService: INotifyPropertyChanged
{
    FileViewModel? SelectedDocument { get; set; }
    ObservableCollection<FileViewModel> Documents { get; }
    ObservableCollection<ToolViewModel> Tools { get; }
    FileStatsViewModel FileStats { get; set; }

    event EventHandler? ActiveDocumentChanged;
    void ToggleToolVisibility(ToolViewModel tool);
    void SetDockingService(IDockingService dockingService);
}