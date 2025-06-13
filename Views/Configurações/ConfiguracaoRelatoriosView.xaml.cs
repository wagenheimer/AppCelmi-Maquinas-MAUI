using AppCelmiMaquinas.ViewModel;

using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Views
{
    public partial class ConfiguracaoRelatoriosView : ContentView
    {
        // Construtor padrão para XAML
        public ConfiguracaoRelatoriosView() : this(AppCelmiMaquinas.MauiProgram.Services?.GetService<ConfiguracaoRelatoriosViewModel>()!)
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
