using System.Globalization;
using System.Windows.Data;
using Ui.Models;

namespace Ui.Helpers;

public class AddressFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Example: format as hexadecimal
        if (value is int intValue)
        {
            return $"0x{intValue:X4}";
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return Binding.DoNothing;
        str = str.Trim();

        if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            str = str[2..]; // Remove "0x"
        }

        return int.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result)
            ? result
            : Binding.DoNothing;
    }
}