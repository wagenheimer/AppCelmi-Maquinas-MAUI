using AppCelmiMaquinas.ViewModel;
using LocalizationResourceManager.Maui;
using AppCelmiMaquinas.Services;

namespace AppCelmiMaquinas.Views;

public partial class ConfiguracaoView : ContentView
{
    public ConfiguracaoView()
    {
        InitializeComponent();


        BindingContext = AppCelmiMaquinas.MauiProgram.Services?.GetService<ConfiguracaoViewModel>();
    }
}