using System.ComponentModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;

public interface ISettingsViewModel
{
    /// <inheritdoc cref="SettingsViewModel._appVersion"/>
    string AppVersion
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_appVersion")]
        set;
    }

    /// <inheritdoc cref="SettingsViewModel._currentApplicationTheme"/>
    global::Wpf.Ui.Appearance.ApplicationTheme CurrentApplicationTheme { get; set; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand{T}"/> instance wrapping <see cref="SettingsViewModel.ChangeTheme"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand<global::Wpf.Ui.Appearance.ApplicationTheme> ChangeThemeCommand { get; }

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}