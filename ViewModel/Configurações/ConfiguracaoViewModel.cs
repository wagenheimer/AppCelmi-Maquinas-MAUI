using LocalizationResourceManager.Maui;
using AppCelmiMaquinas.Services;
using AppCelmiMaquinas.Implementations;
using CommunityToolkit.Mvvm.ComponentModel;
using CelmiBluetooth.ViewModels;

namespace AppCelmiMaquinas.ViewModel
{
    public partial class ConfiguracaoViewModel : ViewModelBase
    {
        [ObservableProperty]
        private AppConfigurationService? appConfigurationService;

        public LanguageSelectorViewModel LanguageSelectorViewModel { get; }


        public ConfiguracaoViewModel(ILocalizationResourceManager resourceManager, LanguageSelectorViewModel languageSelectorViewModel, AppConfigurationService appConfigurationService)
            : base(resourceManager)
        {
            AppConfigurationService = appConfigurationService;

            resourceManager = MauiProgram.Services?.GetService<ILocalizationResourceManager>()!;
            languageSelectorViewModel = MauiProgram.Services?.GetService<LanguageSelectorViewModel>()!;

            LanguageSelectorViewModel = languageSelectorViewModel;
            
        }

        protected override void UpdateLocalizedProperties()
        {
            OnPropertyChanged(nameof(LanguageSelectorViewModel));
        }
    }
}
