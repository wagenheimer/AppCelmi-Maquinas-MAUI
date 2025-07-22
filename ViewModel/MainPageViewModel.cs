using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using CelmiBluetooth.Services.Configuration;
using CelmiBluetooth.Maui.Services.Localizatrion;
using System.Linq;
using CelmiBluetooth.ViewModels;

namespace AppCelmiMaquinas.ViewModel
{
    public partial class MainPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int selectedTabIndex;

        private readonly ICelmiBluetoothConfigurationService _configService;
        public ICelmiLocalizationService Localization { get; }

        public MainPageViewModel(
            ILocalizationResourceManager resourceManager,
            ICelmiBluetoothConfigurationService configService,
            ICelmiLocalizationService localization)
            : base(resourceManager)
        {
            _configService = configService;
            Localization = localization;

            //Evita Tela de Apagar
            DeviceDisplay.Current.KeepScreenOn = true;
        }

        [RelayCommand]
        public async Task ShareAppInfo()
        {
            var shareText = new StringBuilder();
            var langConfig = _configService.GetConfiguration<LanguageConfiguration>();
            var reportConfig = _configService.GetConfiguration<ReportConfiguration>();

            // Add app information
            shareText.AppendLine($"{ResourceManager["AppCelmiPecuária"]}");
            shareText.AppendLine($"{Localization.AppVersionText}");
            shareText.AppendLine();

            // Add current configuration
            shareText.AppendLine($"{ResourceManager["ConfiguraçõesCamelCase"]}:");
            if (langConfig != null)
            {
                shareText.AppendLine($"{ResourceManager["Idioma"]}: {langConfig.CurrentCulture}");
            }

            // Add custom fields if any
            if (reportConfig != null && reportConfig.CustomFields.Any())
            {
                shareText.AppendLine();
                shareText.AppendLine($"{ResourceManager["CamposPersonalizados"]}:");
                foreach (var field in reportConfig.CustomFields)
                {
                    shareText.AppendLine($"- {field.Title}");
                }
            }

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = ResourceManager["AppCelmiPecuária"],
                Text = shareText.ToString(),
                Subject = $"{ResourceManager["AppCelmiPecuária"]} - {ResourceManager["Informações"]}"
            });
        }

        protected override void UpdateLocalizedProperties()
        {
            // Este método é chamado quando a cultura muda.
            // Se MainPageViewModel tivesse suas próprias propriedades localizadas (não vindas de AppConfig),
            // elas seriam atualizadas aqui.
            // Ex: MyLocalizedProperty = ResourceManager.GetValue("MyKey");

            // Como AppVersionText é gerenciado por Localization, que já escuta as mudanças de cultura,
            // não precisamos fazer nada para ele aqui.
        }
    }
}