using System;
using System.Globalization;
using System.Windows.Data;

namespace Client_Management_System_V4.Utilities
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle null value (returns false for IsChecked)
            if (value == null) return false;

            if (value is int intValue && parameter is string paramString)
            {
                if (int.TryParse(paramString, out int paramInt))
                {
                    return intValue == paramInt;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string paramString)
            {
                if (int.TryParse(paramString, out int paramInt))
                {
                    return paramInt;
                }
            }
            // Return a default value (0 = Female) if nothing is selected or conversion fails
            return 0; 
        }
    }
}
