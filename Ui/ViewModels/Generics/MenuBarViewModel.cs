using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Ui.Interfaces.ViewModel;

namespace Ui.ViewModels.Generics;
public partial class MenuBarViewModel : ObservableObject, IMenuBarViewModel
{

    private readonly IActiveDocumentService _documentService;
    public ObservableCollection<FileViewModel> Documents => _documentService.Documents;
    public ObservableCollection<ToolViewModel> Tools { get; } = new();

    [ObservableProperty]
    private FileStatsViewModel _fileStats;

    public FileViewModel? SelectedDocument
    {
        get => _documentService.SelectedDocument;
        set => _documentService.SelectedDocument = value;
    }

    public MenuBarViewModel(IActiveDocumentService documentService, FileStatsViewModel fileStatsViewModel)
    {
        _documentService = documentService;
        
        _fileStats = fileStatsViewModel;
        Tools.Add(fileStatsViewModel);
    }

    [RelayCommand]
    private void NewDocument()
    {
        var doc = new FileViewModel()
        {
            Title = "Untitled",
            Content = "Default start content",
        };
        Documents.Add(doc);
        SelectedDocument = doc;
    }
    [RelayCommand]
    private void OpenDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open File",
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var doc = new FileViewModel();
            doc.LoadFromFile(dialog.FileName);
            Documents.Add(doc);
            SelectedDocument = doc;
        }
    }

}
