using System.IO;
using Ui.Components;

namespace Ui.ViewModels.Generics;

public partial class FileViewModel : PaneViewModel
{
    [ObservableProperty] public override partial string? Title { get; set; } = "Untitled";
    public FileViewModel()
    {
    }

    [ObservableProperty] public partial string Content { get; set; } = string.Empty;

    [ObservableProperty] public partial string? FilePath { get; set; }

    public StyledAvalonEdit? EditorInstance { get; set; }
    public bool IsUserCode = false;
    public async Task LoadFromFile(string path)
    {
        FilePath = path;
        Content = await File.ReadAllTextAsync(path);
        Title = Path.GetFileName(path);
        IsUserCode = true;
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