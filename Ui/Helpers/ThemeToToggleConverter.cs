using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Appearance;

namespace Ui.Helpers;

/// <summary>
/// Converter that toggles between Light and Dark themes
/// </summary>
internal sealed class ThemeToToggleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ApplicationTheme theme)
        {
            return theme == ApplicationTheme.Light ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }
        
        return ApplicationTheme.Light;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}