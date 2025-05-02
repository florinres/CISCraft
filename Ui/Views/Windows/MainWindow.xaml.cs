using System.Windows.Controls;
using System.Windows.Data;
using Ui.Interfaces.Windows;
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

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    // private void OnPropertiesMenuLoaded(object sender, RoutedEventArgs e)
    // {
    //     var menuItem = (MenuItem)sender;
    //     menuItem.SetBinding(MenuItem.IsCheckedProperty, new Binding("Workspace.DocumentService.FileStats.IsVisible") 
    //     { 
    //         Mode = BindingMode.TwoWay 
    //     });
    //
    // }
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var realDockingService = new DockingService(DockingManagerInstance);

        var activeDocumentService = ((IMainWindowViewModel)DataContext).Workspace.ActiveDocumentsService;
        var menuBar = ((IMainWindowViewModel)DataContext).MenuBar;

        // Replace dummy docking service with real one
        activeDocumentService.SetDockingService(realDockingService);
        menuBar.SetDockingService(realDockingService);
        
        menuBar.SetToolsVisibilityOnAndOff();
    }
}