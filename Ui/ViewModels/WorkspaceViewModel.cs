using System.Collections.ObjectModel;
using Microsoft.Win32;
using Ui.Models;
using Ui.Services;
using FileViewModel = Ui.Models.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    private readonly IActiveDocumentService _documentService;
    
    public ObservableCollection<FileViewModel> Documents => _documentService.Documents;
    
    //TODO: we will somehow need to identify each of the tools individually. i would like to do that through a Dictionary at this lever but it probably isn't feasable, so it will need to be done in each 
    // individually. This is something we can enforce through the use of interfaces. But  this will sufice for now.
    public ObservableCollection<object> Tools { get; } = new();
    
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