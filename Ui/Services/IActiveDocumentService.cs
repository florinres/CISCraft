using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Models;

namespace Ui.Services;

public interface IActiveDocumentService: INotifyPropertyChanged
{
    FileViewModel? SelectedDocument { get; set; }
    ObservableCollection<FileViewModel> Documents { get; }
    event EventHandler? ActiveDocumentChanged;
}