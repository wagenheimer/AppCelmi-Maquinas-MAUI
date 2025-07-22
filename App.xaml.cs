using CelmiBluetooth.Services.Configuration;

using Microsoft.Extensions.DependencyInjection;

using Syncfusion.Licensing;

using System.Diagnostics;

namespace AppCelmiMaquinas
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();

                // Força o tema Light sempre
                if (Application.Current != null)
                {
                    Application.Current.UserAppTheme = AppTheme.Light;
                }

                //https://www.syncfusion.com/account/downloads
                SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlcd3VWRmdZWUJwW0FWYEk=");

                VersionTracking.Track();

               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App] ERRO durante inicialização: {ex.Message}");
                Debug.WriteLine($"[App] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                // Get AppShell from dependency injection with null check
                if (MauiProgram.Services == null)
                {
                    throw new InvalidOperationException("Services not initialized");
                }

                var appShell = MauiProgram.Services.GetRequiredService<AppShell>();
                return new Window(appShell);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App] ERRO ao criar Window: {ex.Message}");
                Debug.WriteLine($"[App] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}