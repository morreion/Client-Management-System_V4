using System;
using System.Globalization;
using System.Windows.Data;

namespace Client_Management_System_V4.Utilities
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && int.TryParse(parameter?.ToString(), out int targetValue))
            {
                return intValue == targetValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && int.TryParse(parameter?.ToString(), out int targetValue))
            {
                return targetValue;
            }
            return Binding.DoNothing;
        }
    }
}
