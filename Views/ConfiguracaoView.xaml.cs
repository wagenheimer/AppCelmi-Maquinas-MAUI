using AppCelmiPecuaria.ViewModel;
using LocalizationResourceManager.Maui;
using AppCelmiPecuaria.Services;

namespace AppCelmiPecuaria.Views;

public partial class ConfiguracaoView : ContentView
{
    public ConfiguracaoView()
    {
        InitializeComponent();

        var resourceManager = MauiProgram.Services.GetService<ILocalizationResourceManager>()!;
        var languageSelectorViewModel = MauiProgram.Services.GetService<LanguageSelectorViewModel>()!;
        var appConfig = MauiProgram.Services.GetService<IAppConfigurationService>()!;

        BindingContext = new ConfiguracaoViewModel(resourceManager, languageSelectorViewModel, appConfig);
    }
}