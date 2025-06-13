using AppCelmiMaquinas.Implementations;
using AppCelmiMaquinas.Services;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Licensing;

namespace AppCelmiMaquinas
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Força o tema Light sempre
            Application.Current.UserAppTheme = AppTheme.Light;

            //https://www.syncfusion.com/account/downloads
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlecnVWQmFeWEJxWkJWYUA=\r\n");

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
