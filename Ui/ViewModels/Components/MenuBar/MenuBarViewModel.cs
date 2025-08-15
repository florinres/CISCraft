using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.MenuBar;

public partial class MenuBarViewModel : ObservableObject, IMenuBarViewModel
{
    private readonly IToolVisibilityService _toolVisibilityService;
    private IDockingService _dockingService;
    public static ObservableCollection<FileViewModel> files;
    public static string DefaultFileName = "New";
    [ObservableProperty] public partial ILayoutControlViewModel LayoutControl { get; set; }

    public MenuBarViewModel(IActiveDocumentService documentService, IToolVisibilityService toolVisibilityService, ILayoutControlViewModel layoutControl)
    {
        DocumentService = documentService;
        _toolVisibilityService = toolVisibilityService;
        LayoutControl = layoutControl;
    }

    [ObservableProperty] public partial IActiveDocumentService DocumentService { get; set; }

    public void SetDockingService(IDockingService dockingService)
    {
        _dockingService = dockingService;
        _toolVisibilityService.SetDockingService(dockingService);
        LayoutControl.SetDockingService(dockingService);
        _dockingService.LoadLastUsedLayout();
        files = DocumentService.Documents;
    }

    public void SetToolsVisibilityOnAndOff()
    {
        foreach (var tool in DocumentService.Tools)
        {
            _toolVisibilityService.ToggleToolVisibility(tool);
            _toolVisibilityService.ToggleToolVisibility(tool);
        }
    }

    [RelayCommand]
    private void NewDocument()
    {
        string defaultContent = string.Empty;
        string filePath = string.Empty;
        string fullPath = string.Empty;
        string fileName = string.Empty;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            var projectRoot = Directory.GetCurrentDirectory();
            fullPath = Path.Combine(projectRoot, "Assets", "default_code.txt");
        }
        else
        {
            fullPath = Path.GetFullPath(filePath);
        }
        try
        {
            defaultContent = File.ReadAllText(fullPath);
        }
        catch (IOException)
        {
            defaultContent = fullPath + Static_Messages.FILE_NOT_EXIST_TEXT;
        }
        fileName = GetNextAvailableFileName();

        var doc = new FileViewModel
        {
            Title = fileName,
            Content = defaultContent
        };
        DocumentService.Documents.Add(doc);
        DocumentService.SelectedDocument ??= doc;
    }

    /// <summary>
    /// Generates a new unique file name by finding the highest numeric suffix
    /// among existing documents whose title contains the default file name pattern,
    /// then incrementing it.
    /// </summary>
    /// <returns>A unique file name with an incremented number suffix.</returns>
    private string GetNextAvailableFileName()
    {
        ObservableCollection<FileViewModel> openDocuments = DocumentService.Documents;
        List<FileViewModel> matchingDocuments = new List<FileViewModel>();
        string newFileName;

        foreach (var document in openDocuments)
        {
            if (document.Title.Contains(DefaultFileName))
            {
                matchingDocuments.Add(document);
            }
        }

        int highestSuffixNumber = int.MinValue;
        foreach (var document in matchingDocuments)
        {
            int suffixNumber = int.Parse(document.Title.Substring(3));
            if (suffixNumber > highestSuffixNumber)
            {
                highestSuffixNumber = suffixNumber;
            }
        }

        highestSuffixNumber++;
        if (matchingDocuments.Count == 0)
        {
            highestSuffixNumber = 1;
        }

        newFileName = DefaultFileName + highestSuffixNumber.ToString();
        return newFileName;
    }


    [RelayCommand]
    private async Task OpenDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = Static_Messages.OPEN_FILE_TEXT,
            Filter = Static_Messages.OPEN_FILE_FILTER
        };

        if (dialog.ShowDialog() != true) return;

        var doc = new FileViewModel();
        await doc.LoadFromFile(dialog.FileName);
        DocumentService.Documents.Add(doc);
        DocumentService.SelectedDocument ??= doc;
    }

    [RelayCommand]
    private void ShowFileStats()
    {
        _toolVisibilityService.ToggleToolVisibility(DocumentService.FileStats);
    }
    
    [RelayCommand]
    private void ShowDiagram()
    {
        _toolVisibilityService.ToggleToolVisibility(DocumentService.Diagram);
    }
    
    [RelayCommand]
    public void ShowHexViewer()
    {
        _toolVisibilityService.ToggleToolVisibility(DocumentService.HexViewer);
    }
    
    [RelayCommand]
    public void ShowMicroprogram()
    {
        _toolVisibilityService.ToggleToolVisibility(DocumentService.Microprogram);
    }

    public static void CloseDocument(FileViewModel file)
    {
        string filePath = file.FilePath;
        if (!File.Exists(filePath))
        {
            MessageBoxResult result = MessageBox.Show(
                Static_Messages.SAVE_FILE_TEXT,
                Static_Messages.ATTENTION,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );
            if(result == MessageBoxResult.Yes)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = Static_Messages.SAVE_FILE_LABEL,
                    Filter = Static_Messages.SAVE_FILE_FILTER,
                    DefaultExt = Static_Messages.DEFAULT_EXTENSION,
                    AddExtension = true,
                    FileName = file.Title,
                };
                if(saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, file.Content);
                }
            }
            else if(result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
            {
                return;
            }
            files.Remove(file);
        }   
    }
}