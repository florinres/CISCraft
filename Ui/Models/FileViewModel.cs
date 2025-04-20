using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ui2.Models;

public partial class FileViewModel : ObservableObject
{
    [ObservableProperty] private string title = "Untitled";
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty] private string? filePath;
    [ObservableProperty] private bool isDirty = true;
    
    partial void OnContentChanged(string oldValue, string newValue)
    {
        IsDirty = true;
    }

    public void LoadFromFile(string path)
    {
        FilePath = path;
        Content = File.ReadAllText(path);
        Title = Path.GetFileName(path);
        IsDirty = false;
    }

    public void SaveToFile(string? path = null)
    {
        var targetPath = path ?? FilePath;
        if (targetPath != null)
        {
            File.WriteAllText(targetPath, Content);
            FilePath = targetPath;
            Title = Path.GetFileName(targetPath);
            IsDirty = false;
        }
    }
}