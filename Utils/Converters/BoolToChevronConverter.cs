using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converte um bool (expandido/recolhido) em um ícone de chevron do FontAwesome.
    /// </summary>
    public class BoolToChevronConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                // Chevron-down se expandido, chevron-right se recolhido
                return isExpanded ? "\uf078" : "\uf054"; // FontAwesome Unicode: down/right
            }
            return "\uf054";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
