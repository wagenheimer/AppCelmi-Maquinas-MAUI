using System.Globalization;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converte um valor bool para texto de estabilidade (Estável/Instável)
    /// </summary>
    public class StabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isStable)
            {
                return isStable ? "Estável" : "Instável";
            }
            
            return "Instável";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text == "Estável";
            }
            
            return false;
        }
    }
}