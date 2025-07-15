using System.ComponentModel;

using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Views
{
    public partial class TitleView : ContentView
    {
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(TitleView), string.Empty);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TitleView), string.Empty);
        public static readonly BindableProperty Text2Property = BindableProperty.Create(nameof(Text2), typeof(string), typeof(TitleView), string.Empty);

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Text2
        {
            get => (string)GetValue(Text2Property);
            set => SetValue(Text2Property, value);
        }

        public TitleView()
        {
            InitializeComponent();
        }
    }
}
