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
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Get AppShell from dependency injection
            var appShell = MauiProgram.Services.GetRequiredService<AppShell>();
            return new Window(appShell);
        }
    }
}
