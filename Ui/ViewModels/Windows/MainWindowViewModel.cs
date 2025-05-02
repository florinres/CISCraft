
using Ui.Helpers;
using Ui.Interfaces.ViewModel;
using Ui.Interfaces.Windows;

namespace Ui.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        [ObservableProperty]
        public partial string ApplicationTitle { get; set; } = "WPF UI - Ui";

        [ObservableProperty]
        public partial IWorkspaceViewModel Workspace { get; set; }

        [ObservableProperty]
        public partial IMenuBarViewModel MenuBar { get; set; }

        [ObservableProperty]
        public partial IActionsBarViewModel ActionsBar { get; set; }

        public MainWindowViewModel(IWorkspaceViewModel workspace, IMenuBarViewModel menuBar, IActionsBarViewModel actionsBar)
        {
            Workspace = workspace;
            MenuBar = menuBar;
            ActionsBar = actionsBar;
        }
    }
}
