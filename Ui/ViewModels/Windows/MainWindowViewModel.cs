
using Ui.Helpers;

namespace Ui.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - Ui";
        [ObservableProperty]
        private IWorkspaceViewModel _workspace;

        public MainWindowViewModel(IWorkspaceViewModel workspace)
        {
            _workspace = workspace;
        }
    }
}
