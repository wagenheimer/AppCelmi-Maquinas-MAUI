using AppCelmiMaquinas.ViewModel;
using LocalizationResourceManager.Maui;

namespace AppCelmiMaquinas.Views;

public partial class ConfiguracaoView : ContentView
{
    public ConfiguracaoView()
    {
        InitializeComponent();

        BindingContext = AppCelmiMaquinas.MauiProgram.Services?.GetService<ConfiguracaoViewModel>();
    }
}