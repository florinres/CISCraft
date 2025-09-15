using AvalonDock;
using ICSharpCode.AvalonEdit;
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
using Ui.ViewModels.Components.MenuBar;
using Ui.ViewModels.Generics;
using Wpf.Ui.Appearance;

namespace Ui.Views.Windows;

public partial class MainWindow
{
    ICpuService _cpuService;
    IActionsBarViewModel _actionsBarViewModel;
    public static List<ISR>? Isrs;
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
        IActionsBarViewModel actionsBarViewModel
    )
    {
        DataContext = viewModel;
        SystemThemeWatcher.Watch(this);
        InitializeComponent();
        _cpuService = cpuService;
        _actionsBarViewModel = actionsBarViewModel;

        EditInterruptsMenu.Items.Clear();

        Isrs = ReadIVTJson();

        foreach (var isr in Isrs)
        {
            var menuItem = new Wpf.Ui.Controls.MenuItem
            {
                Header = isr.Name,
                Command = viewModel.MenuBar.EditISRCommand,
                CommandParameter = isr
            };

            EditInterruptsMenu.Items.Add(menuItem);
        }
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
        if (e.Anchorable.Content is not IToolViewModel toolVm) return;
        
        toolVm.IsVisible = false;
        e.Cancel = true;
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
            _cpuService.SetActiveEditor(vm);
            _actionsBarViewModel.CanAssemble = true;
            editor.Unloaded += OnEditorUnloaded;
            if(Isrs != null)
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