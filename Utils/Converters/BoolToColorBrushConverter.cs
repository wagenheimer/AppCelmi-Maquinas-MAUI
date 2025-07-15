using System.Globalization;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converte um valor boolean para um Brush (verde para true, vermelho para false)
    /// </summary>
    public class BoolToColorBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converte um boolean para um Brush
        /// </summary>
        /// <param name="value">Valor boolean a converter</param>
        /// <param name="targetType">Tipo de destino (Brush)</param>
        /// <param name="parameter">Par�metro opcional (n�o utilizado)</param>
        /// <param name="culture">Informa��es de cultura</param>
        /// <returns>Brush verde se true, vermelho se false</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isStable)
            {
                // Obt�m as cores dos recursos da aplica��o
                string colorKey = isStable ? "Green500" : "Red500";

                if (Application.Current?.Resources.TryGetValue(colorKey, out var colorResource) == true)
                {
                    if (colorResource is Color color)
                    {
                        return new SolidColorBrush(color);
                    }
                }

                // Fallback para cores padr�o caso n�o encontre os recursos
                return new SolidColorBrush(isStable ? Colors.Green : Colors.Red);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Converte de volta um Brush para um boolean (n�o implementado)
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Convers�o de volta n�o � necess�ria neste caso
            return false;
        }
    }
}