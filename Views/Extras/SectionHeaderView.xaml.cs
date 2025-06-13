using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Views
{
    public partial class SectionHeaderView : ContentView
    {
        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(SectionHeaderView), string.Empty);

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(SectionHeaderView), string.Empty);

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

        public SectionHeaderView()
        {
            InitializeComponent();
            BindingContext = this;
        }
    }
}
