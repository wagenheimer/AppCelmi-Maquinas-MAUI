using AppCelmiPecuaria.Implementations;
using AppCelmiPecuaria.Services;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Licensing;

namespace AppCelmiPecuaria
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //https://www.syncfusion.com/account/downloads
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NBaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXxfeHVTR2NZWEV0VkI=");

            VersionTracking.Track();

            // Garante que o serviço de configuração seja inicializado.
            // A chamada a Load() foi removida daqui, pois o construtor do AppConfigurationService já a faz.
            var appConfigService = MauiProgram.Services?.GetRequiredService<AppConfigurationService>();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Get AppShell from dependency injection
            var appShell = MauiProgram.Services.GetRequiredService<AppShell>();
            return new Window(appShell);
        }
    }
}
