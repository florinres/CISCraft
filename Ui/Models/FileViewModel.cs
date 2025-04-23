using System.IO;
using Ui.Models.Generics;

namespace Ui.Models;

public partial class FileViewModel : PaneViewModel
{
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private string? _filePath;

    public FileViewModel()
    {
        Title = "Untitled";
    }
    
    public void LoadFromFile(string path)
    {
        FilePath = path;
        Content = File.ReadAllText(path);
        Title = Path.GetFileName(path);
    }

    public void SaveToFile(string? path = null)
    {
        var targetPath = path ?? FilePath;
        if (targetPath != null)
        {
            File.WriteAllText(targetPath, (string?)Content);
            FilePath = targetPath;
            Title = Path.GetFileName(targetPath);
        }
    }
}