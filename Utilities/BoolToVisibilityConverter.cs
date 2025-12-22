using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client_Management_System_V4.Utilities
{
    /// <summary>
    /// Converts Boolean to Visibility (true = Visible, false = Collapsed)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Support Inverted parameter
                string? paramStr = parameter as string;
                if (paramStr != null && paramStr.Equals("Inverted", StringComparison.OrdinalIgnoreCase))
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
