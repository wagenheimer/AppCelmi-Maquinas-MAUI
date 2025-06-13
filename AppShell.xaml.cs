using LocalizationResourceManager.Maui;

namespace AppCelmiMaquinas
{
    public partial class AppShell : Shell
    {
        private readonly ILocalizationResourceManager resourceManager;

        public AppShell(ILocalizationResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
            InitializeComponent();
        }
    }
}
