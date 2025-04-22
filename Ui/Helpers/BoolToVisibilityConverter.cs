using System.Globalization;
using System.Windows.Data;

namespace Ui.Helpers;

/// <summary>
/// Source: http://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter
/// 
/// Implements a Boolean to Visibility converter
/// Use ConverterParameter=true to negate the visibility - boolean interpretation.
/// </summary>
[ValueConversion(typeof(Boolean), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a <seealso cref="Boolean"/> value
    /// into a <seealso cref="Visibility"/> value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isInverted = parameter != null && (bool)parameter;
        var isVisible = value != null && (bool)value;

        if (isVisible)
            return isInverted ? Visibility.Hidden : Visibility.Visible;

        return isInverted ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Converts a <seealso cref="Visibility"/> value
    /// into a <seealso cref="Boolean"/> value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var visibility = value == null ? Visibility.Hidden : (Visibility)value;
        var isInverted = parameter != null && (bool)parameter;

        return (visibility == Visibility.Visible) != isInverted;
    }
}