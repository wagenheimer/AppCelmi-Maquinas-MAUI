using AppCelmiPecuaria.ViewModel;
using LocalizationResourceManager.Maui;
using AppCelmiPecuaria.Services;

namespace AppCelmiPecuaria.Views;

public partial class ConfiguracaoView : ContentView
{
    public ConfiguracaoView()
    {
        InitializeComponent();


        BindingContext = AppCelmiPecuaria.MauiProgram.Services?.GetService<ConfiguracaoViewModel>();
    }
}