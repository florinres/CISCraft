using System.Collections.ObjectModel;
using Microsoft.Win32;
using Ui.Models;
using Ui.Models.Generics;
using Ui.Services;
using FileViewModel = Ui.Models.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel
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
    
    public WorkspaceViewModel(IActiveDocumentService documentService, FileStatsViewModel fileStatsViewModel)
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


    // [RelayCommand(CanExecute = nameof(CanSaveSelectedDocument))]
    // private void SaveDocument()
    // {
    //     SelectedDocument?.SaveToFile();
    // }

    // private bool CanSaveSelectedDocument() => SelectedDocument?.IsDirty == true;
}