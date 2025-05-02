using System.ComponentModel;
using System.IO;
using Ui.Interfaces.Services;

namespace Ui.ViewModels.Generics;

public partial class FileStatsViewModel : ToolViewModel
{

    public FileStatsViewModel()
    {
        IsVisible = false;
    }
    [ObservableProperty]
    public partial long FileSize { get; set; }

    [ObservableProperty]
    public partial DateTime LastModified { get; set; }

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilePath { get; set; } = string.Empty;

    public const string ToolContentId = "FileStatsTool";
    
    public void UpdateStats(FileViewModel? doc)
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
