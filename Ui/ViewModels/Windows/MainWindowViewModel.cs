
namespace Ui.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - Ui";
        [ObservableProperty]
        private WorkspaceViewModel _workspace;

        public MainWindowViewModel(WorkspaceViewModel workspace)
        {
            _workspace = workspace;
        }
    }
}
