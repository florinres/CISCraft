using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Appearance;

namespace Ui.Helpers;

/// <summary>
/// Converter that selects the appropriate icon for the current theme
/// </summary>
internal sealed class ThemeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ApplicationTheme theme)
        {
            return theme == ApplicationTheme.Light ? "DarkTheme24" : "BrightnessHigh24";
        }
        
        return "WeatherSunny24";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}