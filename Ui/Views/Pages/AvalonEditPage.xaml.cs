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
    public AvalonEditPage(AvalonEditViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
        LoadAssembly16BitHighlighting();
    }
    
    private void LoadAssembly16BitHighlighting()
    {
        using var reader = XmlReader.Create(@"Assets\AvalonEdit\Assembly16BitHighlighting.xshd");
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    public AvalonEditViewModel ViewModel { get; }
}