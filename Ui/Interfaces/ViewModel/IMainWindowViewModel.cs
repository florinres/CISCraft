using System.Diagnostics.CodeAnalysis;
using Ui.ViewModels.Windows;

namespace Ui.Interfaces.ViewModel;

public interface IMainWindowViewModel
{
    /// <inheritdoc cref="MainWindowViewModel._applicationTitle" />
    string ApplicationTitle { get; [MemberNotNull("_applicationTitle")] set; }

    /// <inheritdoc cref="MainWindowViewModel._workspace" />
    IWorkspaceViewModel Workspace { get; [MemberNotNull("_workspace")] set; }

    /// <inheritdoc cref="MainWindowViewModel._menuBar" />
    IMenuBarViewModel MenuBar { get; [MemberNotNull("_menuBar")] set; }

    /// <inheritdoc cref="MainWindowViewModel._actionsBar" />
    IActionsBarViewModel ActionsBar { get; [MemberNotNull("_actionsBar")] set; }

    ISettingsViewModel Settings { get; set; }
}