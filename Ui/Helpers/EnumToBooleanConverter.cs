using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Appearance;

namespace Ui.Helpers;

internal class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string enumString)
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");

        if (value != null && !Enum.IsDefined(typeof(ApplicationTheme), value))
            throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");

        var enumValue = Enum.Parse<ApplicationTheme>(enumString);

        return enumValue.Equals(value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string enumString)
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");

        return Enum.Parse<ApplicationTheme>(enumString);
    }
}