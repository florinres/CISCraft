using Ui.ViewModels.Windows;
using Ui2.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Ui.Views.Windows;

public partial class MainWindow
{
    public MainWindow(
        MainWindowViewModel viewModel
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
}