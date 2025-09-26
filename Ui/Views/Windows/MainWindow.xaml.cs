using AvalonDock;
using ICSharpCode.AvalonEdit;
using MainMemory.Business;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Ui.Components;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.Services;
using Ui.ViewModels;
using Ui.ViewModels.Components.MenuBar;
using Ui.ViewModels.Generics;
using Wpf.Ui.Appearance;
using ISR = Ui.Models.ISR;

namespace Ui.Views.Windows;

public partial class MainWindow
{
    ICpuService _cpuService;
    IActionsBarViewModel _actionsBarViewModel;
    public static List<ISR>? Isrs;
    IMainMemory _mainMemory;
    IMainWindowViewModel _viewModel;
    IAssemblerService _assemblerService;
    IActiveDocumentService _activeDocumentService;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    public MainWindow(
        IMainWindowViewModel viewModel,
        ICpuService cpuService,
        IActionsBarViewModel actionsBarViewModel,
        IMainMemory mainMemory,
        IAssemblerService assemblerService,
        IActiveDocumentService activeDocumentService
    )
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        SystemThemeWatcher.Watch(this);
        InitializeComponent();
        _cpuService = cpuService;
        _actionsBarViewModel = actionsBarViewModel;
        _mainMemory = mainMemory;
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;

        EditInterruptsMenu.Items.Clear();
        TriggerInterruptMenu.Items.Clear();

        InitInterrupts();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }

    private IDockingService _docking;
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {

        var activeDocumentService = ((IMainWindowViewModel)DataContext).Workspace.ActiveDocumentsService;
        var menuBar = ((IMainWindowViewModel)DataContext).MenuBar;
        var realDockingService = new DockingService(DockingManagerInstance,activeDocumentService);

        _docking = realDockingService;
        // Replace dummy docking service with real one
        activeDocumentService.SetDockingService(realDockingService);
        menuBar.SetDockingService(realDockingService);

        //Tool visibility workaround.
        //TLDRL: AvalonDock nbeeds all docking tabs to be visible in the beggining to register them. After they are registered we can do whatever we want with them but they need to be visible in the bggining
        menuBar.SetToolsVisibilityOnAndOff();
        
        // Reset the layout to initialize it properly with the new titles
        _docking.SaveLastUsedLayout();
        _docking.LoadLastUsedLayout();
    }
    private void DockManager_AnchorableClosing(object? sender, AnchorableClosingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Closing tool: {e.Anchorable.Title}");

        if (e.Anchorable.Content is IToolViewModel tool)
        {
            System.Diagnostics.Debug.WriteLine($"Before: IsVisible = {tool.IsVisible}");

            tool.IsVisible = false;
            e.Anchorable.Hide();

            System.Diagnostics.Debug.WriteLine($"After: IsVisible = {tool.IsVisible}");
            e.Cancel = true;
        }
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        _docking.SaveLastUsedLayout();
        base.OnClosing(e);
    }
    private void DockingManagerInstance_DocumentClosing(object sender, DocumentClosingEventArgs e)
    {
        if (e.Document.Content is FileViewModel thisFile && Isrs != null)
        {
            if (_actionsBarViewModel.IsDebugging)
            {
                e.Cancel = true;
                return;
            }

            if (thisFile.SectionName != "User_Code")
            {
                SaveInterrupt(thisFile);
                _activeDocumentService.Documents.Remove(thisFile);
                return;
            }

            string filePath = thisFile.FilePath;
            if (!File.Exists(filePath))
            {
                MessageBoxResult result = MessageBox.Show(
                    "Do you want to save the file before closing?",
                    "Warning",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning
                );
                if (result == MessageBoxResult.Yes)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Title = "Save file",
                        Filter = "Assembly Files (*.asm)|*.asm|Text Files (*.txt)|*.txt",
                        DefaultExt = "asm",
                        AddExtension = true,
                        FileName = thisFile.Title,
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(saveFileDialog.FileName, thisFile.Content);
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            _activeDocumentService.Documents.Remove(thisFile);
        }
    }
    private void OnEditorLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is StyledAvalonEdit editor && editor.DataContext is FileViewModel vm)
        {
            vm.EditorInstance = editor;

            _actionsBarViewModel.CanAssemble = false;
            _actionsBarViewModel.CanDebug = true;

            if (((vm.IsModified == false) || vm.NeedsAssemble) && _actionsBarViewModel.IsDebugging == false)
            {
                _actionsBarViewModel.CanDebug = false;
                _actionsBarViewModel.CanAssemble = true;
            }
            
            editor.SaveRequested += (s, args) => SaveCurrentDocument(vm);

            editor.Unloaded += OnEditorUnLoaded;
        }
    }
    private void OnEditorUnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is StyledAvalonEdit editor && editor.DataContext is FileViewModel vm)
        { 
            _actionsBarViewModel.CanAssemble = false; 
            _actionsBarViewModel.CanDebug = false;
            _actionsBarViewModel.CanRun = false;
        }
    }
    private void SaveCurrentDocument(FileViewModel document)
    {
        if (document == null) return;

        if (document.SectionName != "User_Code")
        {
            document.Title = document.Title?.Replace("*", "");
            return;
        }

        if (!string.IsNullOrEmpty(document.FilePath) && File.Exists(document.FilePath))
        {
            document.SaveToFile();
            document.IsModified = false;
            _actionsBarViewModel.CanAssemble = true;
        }
        else
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save file",
                Filter = "Assembly Files (*.asm)|*.asm|Text Files (*.txt)|*.txt",
                DefaultExt = "asm",
                AddExtension = true,
                FileName = document.Title?.Replace("*", ""),
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                document.SaveToFile(saveFileDialog.FileName);
                document.IsModified = false;
                _actionsBarViewModel.CanAssemble = true;
            }
        }
    }
    private void OnTextChanged(object sender, EventArgs e)
    {
        _actionsBarViewModel.CanAssemble = false;
        _actionsBarViewModel.CanDebug = false;

        if (sender is StyledAvalonEdit editor && editor.DataContext is FileViewModel vm)
        {
            vm.IsModified = true;
            vm.NeedsAssemble = true;
            if (!vm.Title.EndsWith("*"))
                vm.Title += "*";
        }
    }
    private void InitInterrupts()
    {
        Isrs = ReadIVTJson();

        foreach (var isr in Isrs)
        {
            AddInterruptsButtonsOnUI(isr);

            // Get ISR object code
            var debugSymbols = _assemblerService.AssembleSourceCodeService(isr.TextCode, isr.ISRAddress);

            _cpuService.UpdateDebugSymbols(isr.TextCode, debugSymbols, isr.ISRAddress);
        }
    }
    private void AddInterruptsButtonsOnUI(ISR isr)
    {
        var editInterruptMenu = new Wpf.Ui.Controls.MenuItem
        {
            Header = isr.Name,
            Command = _viewModel.MenuBar.EditISRCommand,
            CommandParameter = isr
        };
        var triggerInterruptMenu = new Wpf.Ui.Controls.MenuItem
        {
            Header = isr.Name,
            Command = _viewModel.ActionsBar.TriggerInterruptCommand,
            CommandParameter = isr
        };

        EditInterruptsMenu.Items.Add(editInterruptMenu);
        TriggerInterruptMenu.Items.Add(triggerInterruptMenu);
    }
    private List<ISR> ReadIVTJson()
    {
        string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../../");
        string jsonPath = Path.Combine(currentFolder + "Configs", "IVT.json");
        if (!File.Exists(jsonPath))
            return new List<ISR>();
        string json = File.ReadAllText(jsonPath);

        return JsonSerializer.Deserialize<List<ISR>>(json, JsonOpts) ?? new();
    }
    private void SaveInterrupt(FileViewModel activeFile)
    {
        if (Isrs is null) return;

        foreach (var isr in Isrs)
        {
            if (isr.Name == activeFile.Title)
            {
                isr.TextCode = activeFile.Content;

                // Update memory with object code and write debug symbols in correct memory section
                var debugSymbols = _assemblerService.AssembleSourceCodeService(isr.TextCode, isr.ISRAddress);

                _cpuService.UpdateDebugSymbols(isr.TextCode, debugSymbols, isr.ISRAddress);

                // Update json
                WriteIsrsToJson(MainWindow.Isrs);
                break;
            }
        }

        MenuBarViewModel.files.Remove(activeFile);
    }
    public void WriteIsrsToJson(List<ISR> isrs)
    {
        string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../../");
        string jsonPath = Path.Combine(currentFolder + "Configs", "IVT.json");
        var json = JsonSerializer.Serialize(isrs, JsonOpts);
        File.WriteAllText(jsonPath, json);
    }
}