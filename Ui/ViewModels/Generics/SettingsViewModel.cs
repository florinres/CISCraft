using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Ui.Interfaces.ViewModel;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Windows;

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
        SwapThemeBrushes(newValue);
    }

    private void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        // Update the theme if it has been changed elsewhere than in the settings.
        if (CurrentApplicationTheme != currentApplicationTheme)
        {
            CurrentApplicationTheme = currentApplicationTheme;
        }
        else
        {
            // Ensure brushes follow external changes too
            SwapThemeBrushes(currentApplicationTheme);
        }
    }

    private static void SwapThemeBrushes(ApplicationTheme theme)
    {
        var app = Application.Current;
        if (app?.Resources == null) return;

        var dictionaries = app.Resources.MergedDictionaries;

        // Remove any existing theme brushes (light/dark)
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            var src = dictionaries[i].Source?.ToString() ?? string.Empty;
            if (src.EndsWith("Resources/ThemeBrushes.Dark.xaml", StringComparison.OrdinalIgnoreCase) ||
                src.EndsWith("Resources/ThemeBrushes.Light.xaml", StringComparison.OrdinalIgnoreCase))
            {
                dictionaries.RemoveAt(i);
            }
        }

        // Add the selected theme dictionary
        var target = theme == ApplicationTheme.Light
            ? new ResourceDictionary { Source = new Uri("Resources/ThemeBrushes.Light.xaml", UriKind.Relative) }
            : new ResourceDictionary { Source = new Uri("Resources/ThemeBrushes.Dark.xaml", UriKind.Relative) };
        dictionaries.Add(target);
    }

    private static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    }
}