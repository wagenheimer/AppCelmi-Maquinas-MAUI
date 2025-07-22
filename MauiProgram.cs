using AppCelmiMaquinas.Implementations;
using AppCelmiMaquinas.ViewModel;
using CelmiBluetooth.Extensions;
using CelmiBluetooth.Maui.Services.Localizatrion;
using CelmiBluetooth.Services.Configuration;
using CommunityToolkit.Maui;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Shiny;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using AppCelmiMaquinas.Views;
using CelmiBluetooth.Utils;
using CelmiBluetooth.Maui.Resources;


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
                // ✅ USAR A NOVA CONFIGURAÇÃO PERSONALIZADA
                .AddCelmiBluetoothServices<AppCelmiConfiguration>()
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
                // ✅ AppShell deve ser Singleton para manter o estado da navegação
                mauiAppBuilder.Services.AddSingleton<AppShell>();

                // ✅ Registrar a própria classe App para permitir a injeção de dependência nela
                mauiAppBuilder.Services.AddSingleton<App>();

                System.Diagnostics.Debug.WriteLine("[MauiProgram] Serviços registrados com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MauiProgram] ERRO ao registrar serviços: {ex.Message}");
                throw;
            }

            return mauiAppBuilder;
        }

        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder
)
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
                mauiAppBuilder.Services.AddTransient<PesagemView>();

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
}