using System.Collections.ObjectModel;
using Microsoft.Win32;
using FileViewModel = Ui.Models.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    public ObservableCollection<FileViewModel> Documents { get; set; } = new();

    [ObservableProperty]
    private FileViewModel? _selectedDocument;

    [RelayCommand]
    private void NewDocument()
    {
        var doc = new FileViewModel()
        {
            Title = "Untitled",
            Content = "Default start content",
        };
        Documents.Add(doc);
        SelectedDocument = doc;
    }

    [RelayCommand]
    private void OpenDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open File",
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var doc = new FileViewModel();
            doc.LoadFromFile(dialog.FileName);
            Documents.Add(doc);
            SelectedDocument = doc;
        }
    }

    // [RelayCommand(CanExecute = nameof(CanSaveSelectedDocument))]
    // private void SaveDocument()
    // {
    //     SelectedDocument?.SaveToFile();
    // }

    // private bool CanSaveSelectedDocument() => SelectedDocument?.IsDirty == true;
}