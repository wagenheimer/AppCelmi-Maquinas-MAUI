using LocalizationResourceManager.Maui;
using AppCelmiPecuaria.ViewModel;
using Syncfusion.Maui.Toolkit.TabView;

namespace AppCelmiPecuaria
{
    public partial class MainPage : ContentPage
    {
        private ILocalizationResourceManager resourceManager;
        private readonly MainPageViewModel viewModel;

        public MainPage(MainPageViewModel mainPageViewModel, ILocalizationResourceManager resourceManager)
        {
            InitializeComponent();

            BindingContext = mainPageViewModel;
            viewModel = mainPageViewModel;
            this.resourceManager = resourceManager;

            System.Threading.Thread.CurrentThread.CurrentCulture = resourceManager.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = resourceManager.CurrentCulture;

            // Add share button to toolbar
            AddShareButton();
        }

        private void AddShareButton()
        {
            var shareButton = new ToolbarItem
            {
                Text = "Share", // Use a fixed text since there's no resource for it yet
                IconImageSource = "share",
                Order = ToolbarItemOrder.Primary,
                Priority = 0
            };

            shareButton.Clicked += ShareButton_Clicked;
            ToolbarItems.Add(shareButton);
        }

        private async void ShareButton_Clicked(object? sender, EventArgs e)
        {
            // Call the share method directly since we had issues with the generated command
            await viewModel.ShareAppInfo();
        }

        private void tabView_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.TabView.TabSelectionChangedEventArgs e)
        {
            // Update the selected tab index in ViewModel if needed
            if (viewModel != null && sender is SfTabView tabView)
            {
                viewModel.SelectedTabIndex = (int)tabView.SelectedIndex;
            }
        }
    }

}