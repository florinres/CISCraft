using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.MenuBar
{
    public partial class MenuBarViewModel : ObservableObject, IMenuBarViewModel
    {
        private readonly IToolVisibilityService _toolVisibilityService;
        private IDockingService _dockingService;
        private IActionsBarViewModel _actionsBarViewModel;
        private IHexViewModel _hexViewModel;
        private IDiagramViewModel _diagramViewModel;
        public static ObservableCollection<FileViewModel> files;
        [ObservableProperty] public partial ILayoutControlViewModel LayoutControl { get; set; } 

        public MenuBarViewModel(IActiveDocumentService documentService, IToolVisibilityService toolVisibilityService, ILayoutControlViewModel layoutControl, IActionsBarViewModel actionsBarViewModel, IHexViewModel hexViewModel, IDiagramViewModel diagramViewModel)
        {
            DocumentService = documentService;
            _toolVisibilityService = toolVisibilityService;
            LayoutControl = layoutControl;
            _actionsBarViewModel = actionsBarViewModel;
            _diagramViewModel = diagramViewModel;
            _hexViewModel = hexViewModel;
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
        public void SetNumberFormat(NumberFormat format)
        {
            _hexViewModel.SetNumberFormat(format);
            _diagramViewModel.SetNumberFormat(format);
        }
        [RelayCommand]
        private void NewDocument()
        {
            string defaultContent = string.Empty;
            string filePath = "";
            string fullPath = "";

            if (string.IsNullOrWhiteSpace(filePath))
            {
                var projectRoot = Directory.GetCurrentDirectory();
                fullPath = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Examples/default.s");
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
                defaultContent = fullPath + " doesn't exist"; // daca nu apare fisierul pe git trb bagat manual in bin ig 
            }

            var doc = new FileViewModel
            {
                Title = "Untitled",
                Content = defaultContent,
                IsUserCode = true
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
        [RelayCommand]
        public void EditISR(ISR isr)
        {
            var doc = new FileViewModel
            {
                Title = isr.Name,
                Content = isr.TextCode,
                IsUserCode = false
            };

            DocumentService.Documents.Add(doc);
            DocumentService.SelectedDocument ??= doc;
        }
    }
}