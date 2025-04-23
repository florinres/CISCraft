using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Ui.Models;
using Ui.Models.Generics;
using Ui.Services;
using FileViewModel = Ui.Models.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel
{
    private readonly IActiveDocumentService _documentService;
    private readonly Assembler.Business.Assembler _assembler;
    private readonly ILogger<WorkspaceViewModel> _logger;
    
    public ObservableCollection<FileViewModel> Documents => _documentService.Documents;
    public ObservableCollection<ToolViewModel> Tools { get; } = new();
    
    [ObservableProperty]
    private FileStatsViewModel _fileStats;

    public FileViewModel? SelectedDocument
    {
        get => _documentService.SelectedDocument;
        set => _documentService.SelectedDocument = value;
    }
    
    public WorkspaceViewModel(IActiveDocumentService documentService, FileStatsViewModel fileStatsViewModel, Assembler.Business.Assembler assembler, ILogger<WorkspaceViewModel> logger)
    {
        _documentService = documentService;
        
        _fileStats = fileStatsViewModel;
        _assembler = assembler;
        _logger = logger;
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
        SelectedDocument ??= doc;
    }

    [RelayCommand]
    private void OpenDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open File",
            Filter = "Text Files (*.asm)|*.asm|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var doc = new FileViewModel();
            doc.LoadFromFile(dialog.FileName);
            Documents.Add(doc);
            SelectedDocument ??= doc;
        }
    }

    [RelayCommand]
    public void AssembleSelectedFile()
    {
        var assembledCode = _assembler.Assemble(_documentService.SelectedDocument!.Content, out int len);

        // using var memoryStream = new MemoryStream(assembledCode);
        // using var writer = new BinaryWriter(memoryStream, Encoding.Unicode, leaveOpen: true);
        // byte[] bytes = memoryStream.ToArray();

        string result = Encoding.Unicode.GetString(assembledCode);

        var doc = new FileViewModel()
        {
            Title = "Assembled File",
            Content = result,
            IsReadOnly = true,
        };
        Documents.Add(doc);
        SelectedDocument ??= doc;
    }

    // [RelayCommand(CanExecute = nameof(CanSaveSelectedDocument))]
    // private void SaveDocument()
    // {
    //     SelectedDocument?.SaveToFile();
    // }

    // private bool CanSaveSelectedDocument() => SelectedDocument?.IsDirty == true;
}