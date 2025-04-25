using System.ComponentModel;
using Ui.ViewModels.Windows;
using Ui.Interfaces.ViewModel;

namespace Ui.Interfaces.Windows;

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

    /// <inheritdoc cref="MainWindowViewModel._menuBar"/>
    IMenuBarViewModel MenuBar
    {
        get;
        [System.Diagnostics.CodeAnalysis.MemberNotNull("_menuBar")]
        set;
    }
}