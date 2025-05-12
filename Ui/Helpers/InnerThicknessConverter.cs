using System.Globalization;
using System.Windows.Data;

namespace Ui.Helpers;

public class InnerThicknessConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double outerThickness)
            return outerThickness * 0.6; // adjust ratio to your liking
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}