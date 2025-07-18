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
using CelmiBluetooth;
using CelmiBluetooth.ViewModels;
using CelmiBluetooth.Views;
using CelmiBluetooth.Extensions; // ✅ NOVA EXTENSÃO

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
                .AddCelmiBluetoothServices() // ✅ USAR A NOVA EXTENSÃO
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
            // ✅ BLUETOOTH SERVICES MOVIDOS PARA A EXTENSÃO CelmiBluetooth
            
            // Registra os serviços da aplicação
            mauiAppBuilder.Services.AddSingleton<ICelmiLocalizationService, CelmiLocalizationService>();
            mauiAppBuilder.Services.AddSingleton<AppConfigurationService, AppConfigurationService>();
            mauiAppBuilder.Services.AddTransient<AppShell>();

            return mauiAppBuilder;
        }

        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<ConfiguracaoViewModel>();
            builder.Services.AddSingleton<ConfiguracaoRelatoriosViewModel>();
            builder.Services.AddSingleton<LanguageSelectorViewModel>();
            
            // ✅ BLUETOOTH VIEWMODELS MOVIDOS PARA A EXTENSÃO CelmiBluetooth
            // builder.Services.AddTransient<BluetoothViewModel>();
            // builder.Services.AddTransient<PesagemViewModel>();

            // More view-models registered here.

            return builder;
        }

        public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
        {
            mauiAppBuilder.Services.AddSingleton<MainPage>();
            mauiAppBuilder.Services.AddTransient<Views.ConfiguracaoRelatoriosView>();
            
            // ✅ BLUETOOTH VIEWS MOVIDOS PARA A EXTENSÃO CelmiBluetooth
            // mauiAppBuilder.Services.AddTransient<BluetoothView>();
            // mauiAppBuilder.Services.AddTransient<PesagemView>();

            // More views registered here.

            return mauiAppBuilder;
        }
    }
}
