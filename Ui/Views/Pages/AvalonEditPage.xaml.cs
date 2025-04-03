using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Ui.ViewModels.Pages;

namespace Ui.Views.Pages;

public partial class AvalonEditPage : Page
{
    public AvalonEditPage()
    {
        InitializeComponent();
        Initialize();
        LoadAssembly16BitHighlighting();
    }

    private void Initialize()
    {
        var editorBackground = (Brush)FindResource("ApplicationBackgroundBrush");
        var editorForeground = (Brush)FindResource("TextFillColorPrimaryBrush");

        textEditor.Background = editorBackground;
        textEditor.Foreground = editorForeground;
        textEditor.TextArea.Foreground = editorForeground;
        textEditor.TextArea.Background = editorBackground;
    }
    
    void LoadAssembly16BitHighlighting()
    {
        using (XmlReader reader = XmlReader.Create("Assets\\AvalonEdit\\Assembly16BitHighlighting.xshd"))
        {
            textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }

}