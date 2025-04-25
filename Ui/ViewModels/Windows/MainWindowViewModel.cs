
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
        [ObservableProperty]
        private IActionsBarViewModel _actionsBar;

        public MainWindowViewModel(IWorkspaceViewModel workspace, IMenuBarViewModel menuBar, IActionsBarViewModel actionsBar)
        {
            _workspace = workspace;
            _menuBar = menuBar;
            _actionsBar= actionsBar;
        }
    }
}
