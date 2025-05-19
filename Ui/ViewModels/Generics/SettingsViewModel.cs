using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Ui.Interfaces.ViewModel;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Ui.ViewModels.Generics;

public sealed partial class SettingsViewModel : ObservableObject, ISettingsViewModel
{
    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    public partial ApplicationTheme CurrentApplicationTheme { get; set; } = ApplicationTheme.Unknown;

    [RelayCommand]
    private void ChangeTheme(ApplicationTheme theme)
    {
        if (CurrentApplicationTheme != theme)
        {
            CurrentApplicationTheme = theme;
        }
    }
    
    public SettingsViewModel()
    {
        CurrentApplicationTheme = ApplicationThemeManager.GetAppTheme();
        AppVersion = $"{GetAssemblyVersion()}";
        ApplicationThemeManager.Changed += OnThemeChanged;
    }

    partial void OnCurrentApplicationThemeChanged(ApplicationTheme oldValue, ApplicationTheme newValue)
    {
        ApplicationThemeManager.Apply(newValue);
    }

    private void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        // Update the theme if it has been changed elsewhere than in the settings.
        if (CurrentApplicationTheme != currentApplicationTheme)
        {
            CurrentApplicationTheme = currentApplicationTheme;
        }
    }

    private static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    }
}