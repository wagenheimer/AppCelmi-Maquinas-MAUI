using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Views
{
    public enum CollapsibleSectionType
    {
        Titulo,
        SubTitulo
    }

    public partial class CollapsibleSectionView : ContentView
    {
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(CollapsibleSectionView), string.Empty);

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CollapsibleSectionView), string.Empty);

        public static readonly BindableProperty Text2Property = BindableProperty.Create(nameof(Text2), typeof(string), typeof(CollapsibleSectionView), string.Empty);

        public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(CollapsibleSectionView), true,
                propertyChanged: OnIsExpandedPropertyChanged);

        public static readonly BindableProperty ViewProperty = BindableProperty.Create(nameof(View), typeof(View), typeof(CollapsibleSectionView), null);

        public static readonly BindableProperty SectionTypeProperty = BindableProperty.Create(nameof(SectionType), typeof(CollapsibleSectionType), typeof(CollapsibleSectionView), CollapsibleSectionType.SubTitulo);

        private static void OnIsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CollapsibleSectionView view && newValue is bool isExpanded)
            {
                view.ExpanderControl.IsExpanded = isExpanded;
            }
        }

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
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public View View
        {
            get => (View)GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        public CollapsibleSectionType SectionType
        {
            get => (CollapsibleSectionType)GetValue(SectionTypeProperty);
            set => SetValue(SectionTypeProperty, value);
        }

        public CollapsibleSectionView()
        {
            InitializeComponent();

            // Initialize the Content property after control is loaded
            this.Loaded += (s, e) =>
            {
                if (View != null && ExpanderControl != null)
                {
                    ExpanderControl.Content = View;
                }
            };
        }
    }
}
