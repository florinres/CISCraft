using System.IO;

namespace Ui.Models;

public partial class FileViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "Untitled";
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private string? _filePath;
    
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