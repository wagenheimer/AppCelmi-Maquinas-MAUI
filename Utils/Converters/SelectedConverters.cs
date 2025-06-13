using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AppCelmiMaquinas.Utils.Converters
{
    public class SelectedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return Application.Current.Resources.TryGetValue("CorPrincipal", out var cor) ? cor : Colors.Orange;
            return Application.Current.Resources.TryGetValue("ToolbarFont", out var cor2) ? cor2 : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SelectedToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return 1.0;
            return 0.6;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SelectedToOrangeBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return Application.Current.Resources.TryGetValue("CorPrincipal", out var cor) ? cor : new SolidColorBrush(Colors.Orange);
            return Brush.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SelectedToDarkOrangeBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return Application.Current.Resources.TryGetValue("CorPrincipalEscura", out var cor) ? cor : new SolidColorBrush(Colors.DarkOrange);
            return Application.Current.Resources.TryGetValue("Toolbar", out var cor2) ? cor2 : Brush.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SelectedToBrushConverter : BindableObject, IValueConverter
    {
        public static readonly BindableProperty SelectedBrushProperty =
            BindableProperty.Create(nameof(SelectedBrush), typeof(Brush), typeof(SelectedToBrushConverter), Brush.Transparent);

        public static readonly BindableProperty UnselectedBrushProperty =
            BindableProperty.Create(nameof(UnselectedBrush), typeof(Brush), typeof(SelectedToBrushConverter), Brush.Transparent);

        public Brush SelectedBrush
        {
            get => (Brush)GetValue(SelectedBrushProperty);
            set => SetValue(SelectedBrushProperty, value);
        }

        public Brush UnselectedBrush
        {
            get => (Brush)GetValue(UnselectedBrushProperty);
            set => SetValue(UnselectedBrushProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return SelectedBrush;
            return UnselectedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
