
using Ui.Helpers;
using Ui.Interfaces.ViewModel;
using Ui.Interfaces.Windows;

namespace Ui.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - Ui";
        [ObservableProperty]
        private IWorkspaceViewModel _workspace;
        [ObservableProperty]
        private IMenuBarViewModel _menuBar;

        public MainWindowViewModel(IWorkspaceViewModel workspace, IMenuBarViewModel menuBar)
        {
            _workspace = workspace;
            _menuBar = menuBar;
        }
    }
}
