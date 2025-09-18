using AvalonDock;
using ICSharpCode.AvalonEdit;
using MainMemory.Business;
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
        IAssemblerService assemblerService
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
            bool isIsr = false;
            foreach (var isr in Isrs)
            {
                if (thisFile.Title == isr.Name)
                {
                    isIsr = true;
                    break;
                }
            }
            if(!isIsr)
                MenuBarViewModel.closeDocument(thisFile);
        }
        _actionsBarViewModel.IsInterruptSaveButtonVisible = false;
    }
    private void OnEditorLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is StyledAvalonEdit editor && editor.DataContext is FileViewModel vm)
        {
            vm.EditorInstance = editor;
            _actionsBarViewModel.CanAssemble = true;
            editor.Unloaded += OnEditorUnloaded;
            if(Isrs != null && _actionsBarViewModel.IsDebugging != true)
            {
                _actionsBarViewModel.IsInterruptSaveButtonVisible = false;
                foreach (var isr in Isrs)
                {
                    if (vm.Title == isr.Name)
                    {
                        _actionsBarViewModel.IsInterruptSaveButtonVisible = true;
                        break;
                    }
                }
            }
        }
    }
    private void OnTextChanged(object sender, EventArgs e)
    {
        _actionsBarViewModel.CanDebug = false;
        _actionsBarViewModel.CanAssemble = true;
    }
    private void OnEditorUnloaded(object sender, EventArgs e)
    {
        _actionsBarViewModel.CanAssemble = false;
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
}