using LocalizationResourceManager.Maui;

using System.Resources;
using AppCelmiPecuaria.ViewModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace AppCelmiPecuaria
{
    public partial class MainPage : ContentPage
    {
        private ILocalizationResourceManager resourceManager;

        public MainPage(MainPageViewModel mainPageViewModel, ILocalizationResourceManager resourceManager)
        {
            InitializeComponent();

            BindingContext = mainPageViewModel;

            this.resourceManager = resourceManager;

            System.Threading.Thread.CurrentThread.CurrentCulture = resourceManager.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = resourceManager.CurrentCulture;

            // Se ConfiguracaoView for uma aba ou parte da MainPage, 
            // você pode precisar instanciar seu ViewModel aqui ou garantir que ele seja injetado.
            // Exemplo, se ConfiguracaoView é referenciada no XAML da MainPage e precisa de um ViewModel específico:
            // var configViewModel = MauiProgram.Services.GetService<ConfiguracaoViewModel>();
            // this.FindByName<ConfiguracaoView>("MinhaConfigView").BindingContext = configViewModel; 
            // Ou, se a ConfiguracaoView for uma aba, seu ViewModel pode ser gerenciado pelo MainPageViewModel
        }

        private void tabView_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.TabView.TabSelectionChangedEventArgs e)
        {

        }
    }
}

