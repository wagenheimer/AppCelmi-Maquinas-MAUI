using LocalizationResourceManager.Maui;
using AppCelmiPecuaria.Services;
using AppCelmiPecuaria.Implementations;

namespace AppCelmiPecuaria.ViewModel
{
    public partial class ConfiguracaoViewModel : ViewModelBase
    {
        private readonly AppConfigurationService _appConfig;

        public LanguageSelectorViewModel LanguageSelectorViewModel { get; }

        public AppConfigurationService AppConfig => _appConfig;


        public ConfiguracaoViewModel(ILocalizationResourceManager resourceManager, LanguageSelectorViewModel languageSelectorViewModel, AppConfigurationService appConfig)
            : base(resourceManager)
        {
            resourceManager = MauiProgram.Services?.GetService<ILocalizationResourceManager>()!;
            languageSelectorViewModel = MauiProgram.Services?.GetService<LanguageSelectorViewModel>()!;
            appConfig = MauiProgram.Services?.GetService<AppConfigurationService>()!;


            _appConfig = appConfig;
            LanguageSelectorViewModel = languageSelectorViewModel;
            _appConfig.Load();
        }

        protected override void UpdateLocalizedProperties()
        {
            OnPropertyChanged(nameof(LanguageSelectorViewModel));
        }
    }
}
