using AvalonDock;
using Ui.Interfaces.ViewModel;
using Ui.Services;
using Wpf.Ui.Appearance;

namespace Ui.Views.Windows;

public partial class MainWindow
{
    public MainWindow(
        IMainWindowViewModel viewModel
    )
    {
        DataContext = viewModel;
        SystemThemeWatcher.Watch(this);
        InitializeComponent();
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

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var realDockingService = new DockingService(DockingManagerInstance);

        var activeDocumentService = ((IMainWindowViewModel)DataContext).Workspace.ActiveDocumentsService;
        var menuBar = ((IMainWindowViewModel)DataContext).MenuBar;

        // Replace dummy docking service with real one
        activeDocumentService.SetDockingService(realDockingService);
        menuBar.SetDockingService(realDockingService);

        //Tool visibility workaround.
        //TLDRL: AvalonDock nbeeds all docking tabs to be visible in the beggining to register them. After they are registered we can do whatever we want with them but they need to be visible in the bggining
        menuBar.SetToolsVisibilityOnAndOff();
    }

    private void DockManager_AnchorableClosing(object? sender, AnchorableClosingEventArgs e)
    {
        if (e.Anchorable.Content is not IToolViewModel toolVm) return;
        
        toolVm.IsVisible = false;
        e.Cancel = true;
    }
}