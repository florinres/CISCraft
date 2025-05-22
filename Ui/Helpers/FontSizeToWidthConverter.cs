using System.Globalization;
using System.Windows.Data;

namespace Ui.Helpers;

public class FontSizeToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double fontSize && parameter is double baseWidth)
        {
            return baseWidth * fontSize / 12.0; // scale relative to default font size 12
        }
        return DependencyProperty.UnsetValue;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}