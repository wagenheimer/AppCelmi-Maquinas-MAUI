using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converte um valor inteiro (Count) para true se for zero, false caso contrário.
    /// </summary>
    public class ZeroToTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            if (value is int intValue)
                return intValue == 0;
            if (int.TryParse(value.ToString(), out intValue))
                return intValue == 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
