using AppCelmiPecuaria.ViewModel;
using Microsoft.Maui.Controls;

namespace AppCelmiPecuaria.Views
{
    public partial class RelatorioCamposPersonalizadosView : ContentView
    {
        // Construtor padrão para XAML
        public RelatorioCamposPersonalizadosView() : this(AppCelmiPecuaria.MauiProgram.Services?.GetService<RelatorioCamposPersonalizadosViewModel>()!)
        {
        }

        public RelatorioCamposPersonalizadosView(RelatorioCamposPersonalizadosViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public RelatorioCamposPersonalizadosViewModel? VM => BindingContext as RelatorioCamposPersonalizadosViewModel;

    }
}
