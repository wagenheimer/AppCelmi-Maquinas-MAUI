using AppCelmiPecuaria.ViewModel;

using LocalizationResourceManager.Maui;

using Microsoft.Extensions.DependencyInjection;

namespace AppCelmiPecuaria.Views;

public partial class ConfiguracaoView : ContentView
{
    public ConfiguracaoView()
    {
        InitializeComponent();

        BindingContext = new ConfiguracaoViewModel(MauiProgram.Services.GetService<ILocalizationResourceManager>(), MauiProgram.Services.GetService<LanguageSelectorViewModel>());
    }
}