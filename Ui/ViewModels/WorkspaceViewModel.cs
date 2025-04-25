using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Interfaces.Windows;
using Ui.ViewModels.Generics;
using FileViewModel = Ui.ViewModels.Generics.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel
{
    IActiveDocumentService _activeDocument;
    public WorkspaceViewModel(IActiveDocumentService activeDocument, IActionsBarViewModel actionsBar)
    {
        _activeDocument = activeDocument;
        actionsBar.ObjectCodeGenerated += OnObjectCodeGenerated;
    }
    private void OnObjectCodeGenerated(object? sender, byte[] objectCode)
    {
        string result = Encoding.Unicode.GetString(objectCode);
        var doc = new FileViewModel()
        {
            Title = "Assembled File",
            Content = result,
        };
        _activeDocument.Documents.Add(doc);
        _activeDocument.SelectedDocument ??= doc;
    }
}