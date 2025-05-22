using System.Globalization;
using System.Windows.Data;

namespace Ui.Helpers;

public class WidthScaler: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double fontSize && parameter is string factorStr && double.TryParse(factorStr, out double factor))
        {
            return fontSize * factor;
        }
        return 100.0; // fallback
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}