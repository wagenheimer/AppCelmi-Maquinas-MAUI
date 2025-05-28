using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using LocalizationResourceManager.Maui;
using AppCelmiPecuaria.Services;

namespace AppCelmiPecuaria.ViewModel
{
    public partial class MainPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int selectedTabIndex;

        public IAppConfigurationService AppConfig { get; }
        public ICelmiLocalizationService Localization { get; }

        public MainPageViewModel(ILocalizationResourceManager resourceManager, IAppConfigurationService appConfig, ICelmiLocalizationService localization)
            : base(resourceManager)
        {
            AppConfig = appConfig;
            Localization = localization;

            //Evita Tela de Apagar
            DeviceDisplay.Current.KeepScreenOn = true;
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
