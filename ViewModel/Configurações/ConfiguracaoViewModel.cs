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

        public CelmiLanguageSelectorViewModel LanguageSelectorVM { get; }
        public CelmiReportConfigurationViewModel ReportConfigVM { get; }


        public ConfiguracaoViewModel(ILocalizationResourceManager resourceManager, CelmiLanguageSelectorViewModel languageSelectorVM,
        CelmiReportConfigurationViewModel reportConfigVM, AppConfigurationService appConfigurationService)
            : base(resourceManager)
        {
            AppConfigurationService = appConfigurationService;

            resourceManager = MauiProgram.Services?.GetService<ILocalizationResourceManager>()!;

            LanguageSelectorVM = languageSelectorVM;
            ReportConfigVM = reportConfigVM;
        }

        protected override void UpdateLocalizedProperties()
        {
            OnPropertyChanged(nameof(LanguageSelectorVM));
        }
    }
}
