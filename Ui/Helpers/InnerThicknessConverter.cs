using System.Globalization;
using System.Windows.Data;

namespace Ui.Helpers;

public class InnerThicknessConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double outerThickness)
        {
            double ratio = 0.6;
            if (parameter is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
            {
                ratio = p;
            }
            return outerThickness * ratio;
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}