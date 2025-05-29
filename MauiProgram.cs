using AppCelmiPecuaria.Implementations;
using AppCelmiPecuaria.Resources;
using AppCelmiPecuaria.Services;
using AppCelmiPecuaria.ViewModel;

using LocalizationResourceManager.Maui;

using Microsoft.Extensions.Logging;

using Shiny;

using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace AppCelmiPecuaria
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
            // Registra os serviços do Shiny
            mauiAppBuilder.Services.AddBluetoothLE();

            // Registra os serviços da aplicação
            mauiAppBuilder.Services.AddSingleton<ICelmiLocalizationService, CelmiLocalizationService>();
            mauiAppBuilder.Services.AddSingleton<IAppConfigurationService, AppConfigurationService>();
            mauiAppBuilder.Services.AddTransient<AppShell>();

            return mauiAppBuilder;
        }

        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddTransient<LanguageSelectorViewModel>();
            builder.Services.AddTransient<ConfiguracaoViewModel>();

            // More view-models registered here.

            return builder;
        }

        public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
        {
            mauiAppBuilder.Services.AddSingleton<MainPage>();

            // More views registered here.

            return mauiAppBuilder;
        }
    }
}
