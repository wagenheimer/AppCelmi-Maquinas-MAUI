using AppCelmiMaquinas.Implementations;
using AppCelmiMaquinas.Resources;
using AppCelmiMaquinas.Services;
using AppCelmiMaquinas.ViewModel;

using LocalizationResourceManager.Maui;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

using Shiny;

using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using CommunityToolkit.Maui;
using AppCelmiMaquinas.Views;

// ✅ IMPORTAÇÕES PARA O SISTEMA DE CONFIGURAÇÃO
using CelmiBluetooth.Extensions;
using CelmiBluetooth.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiMaquinas
{
    public static class MauiProgram
    {
        public static IServiceProvider? Services { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseShiny()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddFont("FontAwesomeBrands.otf", "FontAwesomeBrands");
                    fonts.AddFont("FontAwesomeLight.otf", "FontAwesomeLight");
                    fonts.AddFont("FontAwesomeRegular.otf", "FontAwesomeRegular");
                    fonts.AddFont("FontAwesomeSolid.otf", "FontAwesomeSolid");
                })
                .UseLocalizationResourceManager(settings =>
                {
                    settings.AddResource(AppResources.ResourceManager);
                    settings.RestoreLatestCulture(true);
                })
                .AddCelmiBluetoothServices() // ✅ USAR O NOVO SISTEMA DE CONFIGURAÇÃO
                .RegisterServices()
                .RegisterViewModels()
                .RegisterViews();

#if DEBUG
            // Configure logging in a way compatible with .NET 9
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
#endif

            var app = builder.Build();
            Services = app.Services;
            return app;
        }

        public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
        {
            try
            {
                // ✅ BLUETOOTH SERVICES AGORA SÃO GERENCIADOS PELA EXTENSÃO CelmiBluetooth

                // Registra os serviços da aplicação
                mauiAppBuilder.Services.AddSingleton<ICelmiLocalizationService, CelmiLocalizationService>();
                mauiAppBuilder.Services.AddSingleton<AppConfigurationService>();
                mauiAppBuilder.Services.AddTransient<AppShell>();

                System.Diagnostics.Debug.WriteLine("[MauiProgram] Serviços registrados com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MauiProgram] ERRO ao registrar serviços: {ex.Message}");
                throw;
            }

            return mauiAppBuilder;
        }


        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            try
            {
                builder.Services.AddSingleton<MainPageViewModel>();
                builder.Services.AddSingleton<ConfiguracaoViewModel>();

                // ✅ BLUETOOTH VIEWMODELS AGORA SÃO GERENCIADOS PELA EXTENSÃO CelmiBluetooth

                System.Diagnostics.Debug.WriteLine("[MauiProgram] ViewModels registrados com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MauiProgram] ERRO ao registrar ViewModels: {ex.Message}");
                throw;
            }

            return builder;
        }

        public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
        {
            try
            {
                mauiAppBuilder.Services.AddSingleton<MainPage>();

                // ✅ BLUETOOTH VIEWS AGORA SÃO GERENCIADOS PELA EXTENSÃO CelmiBluetooth

                System.Diagnostics.Debug.WriteLine("[MauiProgram] Views registradas com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MauiProgram] ERRO ao registrar Views: {ex.Message}");
                throw;
            }

            return mauiAppBuilder;
        }
    }

    // ✅ EXEMPLO DE CONFIGURAÇÃO PERSONALIZADA PARA O PROJETO
    public partial class AppCelmiConfiguration : CelmiBluetoothConfiguration
    {
        protected override string StorageKey => "AppCelmiMaquinasSettings";

        /// <summary>
        /// Configurações específicas do App Celmi Máquinas.
        /// </summary>
        [ObservableProperty]
        private AppSpecificSettings appSpecificSettings = new();

        protected override void SetDefaultValues()
        {
            base.SetDefaultValues();

            // Configurar valores específicos do app
            AppSpecificSettings = new AppSpecificSettings
            {
                AppMode = "Maquinas",
                DatabasePath = "AppCelmi.db",
                MaxRecordsPerPage = 50,
                EnableAdvancedFeatures = true
            };

            // Configurar timeouts específicos para máquinas
            BluetoothConfiguration.ConnectionTimeoutSeconds = 20;
            BluetoothConfiguration.ScanTimeoutSeconds = 15;

            // Configurar logging mais detalhado
            LoggingConfiguration.DetailedLoggingEnabled = true;
            LoggingConfiguration.DataLoggingEnabled = true;
        }

        protected override bool ValidateSpecificSettings()
        {
            return base.ValidateSpecificSettings() &&
                   AppSpecificSettings?.Validate() == true;
        }
    }

    /// <summary>
    /// Configurações específicas do App Celmi Máquinas.
    /// </summary>
    public partial class AppSpecificSettings : ObservableObject
    {
        [ObservableProperty]
        private string appMode = "Maquinas";

        [ObservableProperty]
        private string databasePath = "AppCelmi.db";

        [ObservableProperty]
        private int maxRecordsPerPage = 50;

        [ObservableProperty]
        private bool enableAdvancedFeatures = false;

        [ObservableProperty]
        private string backupPath = string.Empty;

        [ObservableProperty]
        private bool autoBackupEnabled = false;

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(AppMode) &&
                   !string.IsNullOrWhiteSpace(DatabasePath) &&
                   MaxRecordsPerPage > 0;
        }
    }
}