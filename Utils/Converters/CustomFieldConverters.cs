using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using LocalizationResourceManager.Maui;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using CelmiBluetooth.Devices;

namespace AppCelmiMaquinas.Utils.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var parameters = parameter.ToString().Split(',');
            var enumValue = value.ToString();

            return parameters.Contains(enumValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type enumType && enumType.IsEnum)
            {
                return Enum.GetValues(enumType)
                          .Cast<object>()
                          .Select(e => e.ToString())
                          .ToList();
            }

            return new List<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && !string.IsNullOrEmpty(strValue))
            {
                return Enum.Parse(targetType, strValue);
            }

            return null;
        }
    }

    public class TranslationKeyToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key && !string.IsNullOrWhiteSpace(key))
            {
                var resourceManager = Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(ILocalizationResourceManager)) as ILocalizationResourceManager;
                if (resourceManager != null)
                {
                    var translated = resourceManager.GetValue(key);
                    if (!string.IsNullOrWhiteSpace(translated))
                        return translated;
                }
                return key;
            }
            return parameter ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomFieldMoveUpEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppCelmiMaquinas.Models.CustomField field && parameter is ObservableCollection<AppCelmiMaquinas.Models.CustomField> list)
            {
                var index = list.IndexOf(field);
                return index > 0 && !field.IsDefault && !list[index - 1].IsDefault;
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CustomFieldMoveDownEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppCelmiMaquinas.Models.CustomField field && parameter is ObservableCollection<AppCelmiMaquinas.Models.CustomField> list)
            {
                var index = list.IndexOf(field);
                return index >= 0 && index < list.Count - 1 && !field.IsDefault && !list[index + 1].IsDefault;
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class UpdateWarningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime lastUpdate)
            {
                return (DateTime.Now - lastUpdate).TotalSeconds > 2;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FontSizeByTextLengthConverter : IValueConverter
    {
        /// <summary>
        /// Converte o texto para um tamanho de fonte que garante que caiba no espaço disponível
        /// </summary>
        /// <param name="value">O texto a ser exibido</param>
        /// <param name="parameter">Formato: "baseFontSize:availableWidth" (ex: "28:120")</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text || string.IsNullOrEmpty(text))
                return 28.0;

            var cleanText = text.Trim();
            if (string.IsNullOrEmpty(cleanText))
                return 28.0;

            // Parse dos parâmetros: "baseFontSize:availableWidth"
            var baseFontSize = 28.0;
            var availableWidth = 120.0; // Largura padrão estimada

            if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr))
            {
                var parts = paramStr.Split(':');
                if (parts.Length >= 1 && double.TryParse(parts[0], out var fontSize))
                    baseFontSize = fontSize;
                if (parts.Length >= 2 && double.TryParse(parts[1], out var width))
                    availableWidth = width;
            }

            // Calcula o tamanho ideal da fonte
            return CalculateOptimalFontSize(cleanText, baseFontSize, availableWidth);
        }

        /// <summary>
        /// Calcula o tamanho ideal da fonte para o texto caber na largura disponível
        /// </summary>
        private double CalculateOptimalFontSize(string text, double baseFontSize, double availableWidth)
        {
            const double minFontSize = 12.0;
            double maxFontSize = baseFontSize;

            // Se o texto é muito curto, usa o tamanho base
            if (text.Length <= 6)
                return maxFontSize;

            // Estimativa aproximada da largura do texto
            // Diferentes caracteres têm larguras diferentes
            var estimatedWidth = EstimateTextWidth(text, baseFontSize);

            // Se cabe no tamanho base, retorna o tamanho base
            if (estimatedWidth <= availableWidth)
                return maxFontSize;

            // Calcula a proporção necessária
            var ratio = availableWidth / estimatedWidth;
            var calculatedSize = baseFontSize * ratio;

            // Garante que não seja menor que o mínimo nem maior que o máximo
            return Math.Max(minFontSize, Math.Min(maxFontSize, calculatedSize));
        }

        /// <summary>
        /// Estima a largura do texto baseada no tamanho da fonte
        /// </summary>
        private double EstimateTextWidth(string text, double fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Largura média aproximada de caracteres por tamanho de fonte
            // Baseado em fontes comuns (Arial, Helvetica)
            const double avgCharWidthRatio = 0.6; // 60% da altura da fonte

            var totalWidth = 0.0;

            foreach (char c in text)
            {
                var charWidth = GetCharacterWidthRatio(c) * fontSize * avgCharWidthRatio;
                totalWidth += charWidth;
            }

            return totalWidth;
        }

        /// <summary>
        /// Retorna a proporção de largura relativa de diferentes caracteres
        /// </summary>
        private double GetCharacterWidthRatio(char c)
        {
            return c switch
            {
                // Caracteres muito estreitos
                'i' or 'j' or 'l' or '|' or 'I' => 0.3,
                'f' or 't' or 'r' => 0.4,

                // Caracteres estreitos
                '1' or ' ' or '.' or ',' or ':' or ';' => 0.5,

                // Caracteres muito largos
                'W' or 'M' => 1.4,
                'w' or 'm' => 1.2,

                // Caracteres largos
                'D' or 'O' or 'Q' or 'G' or 'C' => 1.1,
                'A' or 'V' or 'X' or 'Y' or 'Z' => 1.0,

                // Números (geralmente uniformes)
                >= '0' and <= '9' => 0.8,

                // Caracteres padrão
                _ => 0.9
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte a lista de plataformas e o número da rede configurada em um resumo do tipo:
    /// "Encontradas na rede X: Y de Z plataformas configuradas"
    /// </summary>
    public class PlataformasDaRedeResumoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ObservableCollection<BleDevice> plataformas)
                return string.Empty;
            int redeConfig = 0;
            if (parameter is int p)
                redeConfig = p;
            else if (parameter != null && int.TryParse(parameter.ToString(), out var parsed))
                redeConfig = parsed;

            int totalConfig = plataformas.Count;
            int totalNaRede = plataformas.Count(x => x.RedeComoNumero == redeConfig);

            return $"Encontradas na rede {redeConfig}: {totalNaRede} de {totalConfig} plataformas configuradas";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Retorna 1.0 se a rede do item for igual à configurada, senão 0.4 (apagado)
    /// </summary>
    public class RedeIgualConfiguradaToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0.4;
            int redeItem = 0, redeConfig = 0;
            int.TryParse(value.ToString(), out redeItem);
            int.TryParse(parameter.ToString(), out redeConfig);
            return redeItem == redeConfig ? 1.0 : 0.4;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Retorna CorPrincipalClara se a rede do item for igual à configurada, senão Branco.
    /// </summary>
    public class RedeIgualConfiguradaToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Application.Current.Resources["White"];
            int redeItem = 0, redeConfig = 0;
            int.TryParse(value.ToString(), out redeItem);
            int.TryParse(parameter.ToString(), out redeConfig);
            return redeItem == redeConfig
                ? Application.Current.Resources["CorPrincipalClara"]
                : Application.Current.Resources["White"];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// MultiValueConverter que compara rede do item com rede configurada para retornar cor de fundo.
    /// Valores: [0] = RedeComoNumero do item, [1] = SelectedNetworkNumber
    /// </summary>
    public class RedeBackgroundMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return Application.Current?.Resources?["White"] ?? Colors.White;
            
            if (int.TryParse(values[0].ToString(), out var redeItem) && 
                int.TryParse(values[1].ToString(), out var redeConfig))
            {
                return redeItem == redeConfig
                    ? Application.Current?.Resources?["CorPrincipalClara"] ?? Colors.LightBlue
                    : Application.Current?.Resources?["White"] ?? Colors.White;
            }
            
            return Application.Current?.Resources?["White"] ?? Colors.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}