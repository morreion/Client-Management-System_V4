using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Client_Management_System_V4.Utilities
{
    /// <summary>
    /// Converts a byte array to a BitmapImage for display in Image controls.
    /// Used for displaying images stored as BLOB data in the database.
    /// </summary>
    public class ByteArrayToImageConverter : IValueConverter
    {
        /// <summary>
        /// Converts a byte array to a BitmapImage
        /// </summary>
        /// <param name="value">The byte array containing image data</param>
        /// <param name="targetType">The type to convert to (not used)</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>A BitmapImage if conversion succeeds, null otherwise</returns>
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the value is null or not a byte array, return null
            if (value is not byte[] bytes || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                // Create a BitmapImage from the byte array
                var image = new BitmapImage();
                using (var stream = new MemoryStream(bytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze(); // Freeze for cross-thread access
                }
                return image;
            }
            catch
            {
                // If image creation fails (e.g., PDF data), return null
                return null;
            }
        }

        /// <summary>
        /// Converts a BitmapImage back to a byte array (not typically needed)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported.");
        }
    }
}
