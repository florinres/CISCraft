using System.IO;
using System.Windows.Media;
using Ui.Components;
using Ui.Models;

namespace Ui.ViewModels.Generics;

public partial class FileViewModel : PaneViewModel
{
    Color _semiTransparentYellow;
    public bool IsModified = false;
    public string SectionName = "User_Code";
    private int? _pendingHighlightLine = null;

    [ObservableProperty]
    public partial bool NeedsAssemble { get; set; } = true;
    private HighlightCurrentLineBackgroundRenderer? _highlight
    {
        get;
        set;
    }

    [ObservableProperty] public override partial string? Title { get; set; } = "Untitled";
    public FileViewModel()
    {
    }

    [ObservableProperty] public partial string Content { get; set; } = string.Empty;

    [ObservableProperty] public partial string? FilePath { get; set; }

    private StyledAvalonEdit? _editorInstance;
    public StyledAvalonEdit? EditorInstance
    {
        get => _editorInstance;
        set
        {
            if (_editorInstance == value)
                return;

            _editorInstance = value;

            if (_editorInstance != null)
            {
                _semiTransparentYellow = Color.FromArgb(38, 255, 255, 0);
                _highlight = new HighlightCurrentLineBackgroundRenderer(
                    _editorInstance, 0, _semiTransparentYellow);

                _editorInstance.TextArea.TextView.BackgroundRenderers.Add(_highlight);
                
                // Apply any pending highlight now that the editor is ready
                if (_pendingHighlightLine.HasValue)
                {
                    int lineToHighlight = _pendingHighlightLine.Value;
                    _pendingHighlightLine = null;
                    HighlightLine(lineToHighlight);
                }
            }
        }
    }
    public async Task LoadFromFile(string path)
    {
        FilePath = path;
        Content = await File.ReadAllTextAsync(path);
        Title = Path.GetFileName(path);
    }
    public void HighlightLine(int lineNumber)
    {
        if (_highlight == null)
        {
            // Store the highlight request to apply when editor is ready
            _pendingHighlightLine = lineNumber;
            return;
        }
        _highlight.SetLine(lineNumber);
        EditorInstance?.ScrollTo(lineNumber, 1);
    }
    public void ResetHighlight()
    {
        if (_highlight == null) return;
        _highlight.SetLine(0);
        EditorInstance?.ScrollTo(1, 1);
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