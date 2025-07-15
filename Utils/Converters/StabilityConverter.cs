using System.Globalization;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converte um valor bool para texto de estabilidade (Est�vel/Inst�vel)
    /// </summary>
    public class StabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isStable)
            {
                return isStable ? "Est�vel" : "Inst�vel";
            }
            
            return "Inst�vel";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text == "Est�vel";
            }
            
            return false;
        }
    }
}