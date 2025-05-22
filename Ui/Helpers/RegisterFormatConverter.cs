using System.Globalization;
using System.Windows.Data;
using Ui.Models;

namespace Ui.Helpers;

public class RegisterFormatConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not short value || values[1] is not NumberFormat format)
            return Binding.DoNothing;

        return format switch
        {
            NumberFormat.Binary => System.Convert.ToString(value, 2).PadLeft(16, '0')
                .Insert(4, " ").Insert(9, " ").Insert(14, " ")
                .Insert(4, " ").Insert(9, " ").Insert(14, " "),
            NumberFormat.Hex => $"0x{value:X4}",
            _ => value.ToString()
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}