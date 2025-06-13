using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using LocalizationResourceManager.Maui;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
}