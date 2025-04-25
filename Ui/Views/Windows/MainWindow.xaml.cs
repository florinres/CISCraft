using Ui.Interfaces.Windows;
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
}