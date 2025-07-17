using System.Globalization;

namespace AppCelmiMaquinas.Utils.Converters
{
    /// <summary>
    /// Converte um valor boolean para cores baseadas no par�metro fornecido
    /// </summary>
    public class BoolToColorBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converte um boolean para um Brush
        /// </summary>
        /// <param name="value">Valor boolean a converter</param>
        /// <param name="targetType">Tipo de destino (Brush)</param>
        /// <param name="parameter">Par�metro que define o tipo de convers�o: "Selected" para background, "Text" para texto</param>
        /// <param name="culture">Informa��es de cultura</param>
        /// <returns>Brush baseado no valor boolean e par�metro</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string parameterValue)
            {
                switch (parameterValue.ToLower())
                {
                    case "selected":
                        // Para background do bot�o
                        var backgroundColorKey = boolValue ? "CorPrincipal" : "CorPrincipalClara";
                        if (Application.Current?.Resources.TryGetValue(backgroundColorKey, out var backgroundResource) == true)
                        {
                            if (backgroundResource is Color backgroundColor)
                            {
                                return new SolidColorBrush(backgroundColor);
                            }
                        }
                        return new SolidColorBrush(boolValue ? Color.FromArgb("#FF6A00") : Color.FromArgb("#FFE5D5"));

                    case "text":
                        // Para cor do texto
                        var textColorKey = boolValue ? "White" : "CorPrincipal";
                        if (Application.Current?.Resources.TryGetValue(textColorKey, out var textResource) == true)
                        {
                            if (textResource is Color textColor)
                            {
                                return textColor;
                            }
                        }
                        return boolValue ? Colors.White : Color.FromArgb("#FF6A00");

                    case "iconcolor":
                        // Para cor do �cone (retorna Color, n�o Brush)
                        var iconColorKey = boolValue ? "White" : "CorPrincipal";
                        if (Application.Current?.Resources.TryGetValue(iconColorKey, out var iconResource) == true)
                        {
                            if (iconResource is Color iconColor)
                            {
                                return iconColor;
                            }
                        }
                        return boolValue ? Colors.White : Color.FromArgb("#FF6A00");

                    default:
                        // Comportamento original para compatibilidade
                        var originalColorKey = boolValue ? "Green500" : "Red500";
                        if (Application.Current?.Resources.TryGetValue(originalColorKey, out var originalResource) == true)
                        {
                            if (originalResource is Color originalColor)
                            {
                                return new SolidColorBrush(originalColor);
                            }
                        }
                        return new SolidColorBrush(boolValue ? Colors.Green : Colors.Red);
                }
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