using System.ComponentModel;

namespace Ui.ViewModels.Windows;

public interface IMainWindowViewModel
{
    /// <inheritdoc cref="MainWindowViewModel._applicationTitle"/>
    string ApplicationTitle
    {
        get;
        [System.Diagnostics.CodeAnalysis.MemberNotNull("_applicationTitle")]
        set;
    }

    /// <inheritdoc cref="MainWindowViewModel._workspace"/>
    IWorkspaceViewModel Workspace
    {
        get;
        [System.Diagnostics.CodeAnalysis.MemberNotNull("_workspace")]
        set;
    }

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}