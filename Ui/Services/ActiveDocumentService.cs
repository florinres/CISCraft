using System.Collections.ObjectModel;
using Ui.Interfaces.Services;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class ActiveDocumentService : ObservableObject, IActiveDocumentService
{
    private FileViewModel? _selectedDocument;

    public FileViewModel? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (SetProperty(ref _selectedDocument, value))
            {
                ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public ObservableCollection<FileViewModel> Documents { get; } = new();

    public event EventHandler? ActiveDocumentChanged;
}