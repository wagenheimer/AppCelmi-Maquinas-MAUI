using System.Globalization;
using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converts a byte array to an ImageSource for display in XAML.
    /// </summary>
    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Converts a byte array to an ImageSource.
        /// </summary>
        /// <param name="value">The byte array containing the image data.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>An ImageSource if conversion is successful; otherwise, null.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is not byte[] imageData || imageData.Length == 0)
                {
                    return null;
                }

                return ImageSource.FromStream(() => new MemoryStream(imageData));
            }
            catch (Exception)
            {
                // Return null if there's any error processing the image data
                return null;
            }
        }

        /// <summary>
        /// Converts back from ImageSource to byte array (not implemented).
        /// </summary>
        /// <exception cref="NotImplementedException">This conversion is not supported.</exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}