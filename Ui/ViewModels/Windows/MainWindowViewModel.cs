using Ui.Interfaces.ViewModel;

namespace Ui.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject, IMainWindowViewModel
{
    public MainWindowViewModel(IWorkspaceViewModel workspace, IMenuBarViewModel menuBar,
        IActionsBarViewModel actionsBar, ISettingsViewModel settings)
    {
        Workspace = workspace;
        MenuBar = menuBar;
        ActionsBar = actionsBar;
        Settings = settings;
    }

    [ObservableProperty] public partial string ApplicationTitle { get; set; } = "WPF UI - Ui";

    [ObservableProperty] public partial IWorkspaceViewModel Workspace { get; set; }

    [ObservableProperty] public partial IMenuBarViewModel MenuBar { get; set; }

    [ObservableProperty] public partial IActionsBarViewModel ActionsBar { get; set; }
    
    [ObservableProperty] public partial ISettingsViewModel Settings { get; set; }
}