using AppCelmiPecuaria.ViewModel;

using Microsoft.Maui.Controls;

namespace AppCelmiPecuaria.Views
{
    public partial class ConfiguracaoRelatoriosView : ContentView
    {
        // Construtor padrão para XAML
        public ConfiguracaoRelatoriosView() : this(AppCelmiPecuaria.MauiProgram.Services?.GetService<ConfiguracaoRelatoriosViewModel>()!)
        {
        }

        public ConfiguracaoRelatoriosView(ConfiguracaoRelatoriosViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public ConfiguracaoRelatoriosViewModel? VM => BindingContext as ConfiguracaoRelatoriosViewModel;

    }
}
