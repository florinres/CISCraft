using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Services;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.MenuBar;

public partial class MenuBarViewModel : ObservableObject, IMenuBarViewModel
{
    private readonly IToolVisibilityService _toolVisibilityService;
    private IDockingService _dockingService;
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
        string filePath = "";
        string fullPath;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            var projectRoot = AppContext.BaseDirectory;
            fullPath = Path.Combine(projectRoot, "Assets", "codDefault.txt");
        }
        else
        {
            fullPath = Path.GetFullPath(filePath);
        }
        try
        {
            defaultContent = File.ReadAllText(fullPath); 
        }
        catch (Exception ex)
        {
            
            defaultContent = fullPath + " doesn't exist"; // daca nu apare fisierul pe git trb bagat manual in bin ig 
        }

        var doc = new FileViewModel
        {
            Title = "Microprogram",
            Content = defaultContent
        };
        DocumentService.Documents.Add(doc);
        DocumentService.SelectedDocument ??= doc;
    }

    [RelayCommand]
    private async Task OpenDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open File",
            Filter = "Assembly Files (*.asm;*.s)|*.asm;*.s|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
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
}