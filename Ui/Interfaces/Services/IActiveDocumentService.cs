using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Generics;

public interface IActiveDocumentService: INotifyPropertyChanged
{
    FileViewModel? SelectedDocument { get; set; }
    ObservableCollection<FileViewModel> Documents { get; }
    event EventHandler? ActiveDocumentChanged;
}