using System.ComponentModel;
using System.IO;

namespace Ui.ViewModels.Generics;

public partial class FileStatsViewModel : ToolViewModel
{
    private readonly IActiveDocumentService _documentService;

    public FileStatsViewModel(IActiveDocumentService documentService)
    {
        _documentService = documentService;
        _documentService.PropertyChanged += OnDocumentChanged;
        // initial population
        UpdateStats(_documentService.SelectedDocument);
    }
    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private DateTime _lastModified;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    public const string ToolContentId = "FileStatsTool";

    private void OnDocumentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IActiveDocumentService.SelectedDocument))
        {
            UpdateStats(_documentService.SelectedDocument);
        }
    }

    private void UpdateStats(FileViewModel? doc)
    {
        if (doc?.FilePath != null && File.Exists(doc.FilePath))
        {
            var fi = new FileInfo(doc.FilePath);
            FileSize = fi.Length;
            LastModified = fi.LastWriteTime;
            FileName = fi.Name;
            FilePath = fi.DirectoryName ?? string.Empty;
        }
        else
        {
            FileSize = 0;
            LastModified = DateTime.MinValue;
            FileName = string.Empty;
            FilePath = string.Empty;
        }
    }

}
