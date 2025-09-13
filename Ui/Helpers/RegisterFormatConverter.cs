using System.Globalization;
using System.Windows.Data;
using Ui.Models;

namespace Ui.Helpers;

public class RegisterFormatConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not (short or long or byte) || values[1] is not NumberFormat format)
            return Binding.DoNothing;
        
        long value = System.Convert.ToInt64(values[0]);

        int totalBits = 0;

        switch (values[0])
        {
            case byte:
                totalBits = 8;
                break;
            case short:
                totalBits = 16;
                break;
            case int:
                totalBits = 32;
                break;
            case long:
                totalBits = 64;
                break;
        }

        return format switch
        {
            NumberFormat.Binary => System.Convert.ToString(value, 2).PadLeft(totalBits, '0'),
            NumberFormat.Hex => "0x" + value.ToString("X" + totalBits / 4),
            _ => value.ToString()
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}