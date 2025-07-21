using LocalizationResourceManager.Maui;
using AppCelmiMaquinas.ViewModel;
using Syncfusion.Maui.Toolkit.TabView;
using CelmiBluetooth.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AppCelmiMaquinas
{
    public partial class MainPage : ContentPage
    {
        private readonly ILocalizationResourceManager resourceManager;
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

            // ✅ CONFIGURAR O BLUETOOTH VIEW COM DI E TRATAMENTO DE ERRO
            ConfigurarBluetoothView();
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
            try
            {
                // Call the share method directly since we had issues with the generated command
                await viewModel.ShareAppInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainPage] Erro ao compartilhar: {ex.Message}");
                // Opcional: Mostrar mensagem de erro para o usuário
                await DisplayAlert("Erro", "Erro ao compartilhar informações.", "OK");
            }
        }

        private void tabView_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.TabView.TabSelectionChangedEventArgs e)
        {
            try
            {
                // Update the selected tab index in ViewModel if needed
                if (viewModel != null && sender is SfTabView tabView)
                {
                    viewModel.SelectedTabIndex = (int)tabView.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainPage] Erro ao alterar tab: {ex.Message}");
            }
        }

        private void ConfigurarBluetoothView()
        {
            try
            {
                Debug.WriteLine("[MainPage] Iniciando configuração do BluetoothView");

                // Verificar se o Services Provider está disponível
                if (MauiProgram.Services == null)
                {
                    Debug.WriteLine("[MainPage] ERRO: MauiProgram.Services é null");
                    ConfigurarBluetoothViewFallback();
                    return;
                }

                // Tentar obter o BluetoothView via DI
                var bluetoothView = MauiProgram.Services.GetService<BluetoothView>();

                if (bluetoothView != null)
                {
                    Debug.WriteLine("[MainPage] BluetoothView obtido com sucesso via DI");
                    bluetoothViewContent.Content = bluetoothView;
                }
                else
                {
                    Debug.WriteLine("[MainPage] BluetoothView não foi encontrado via DI, criando manualmente");
                    ConfigurarBluetoothViewFallback();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainPage] ERRO ao configurar BluetoothView: {ex.Message}");
                Debug.WriteLine($"[MainPage] StackTrace: {ex.StackTrace}");

                // Fallback: criar uma view de erro ou view básica
                ConfigurarBluetoothViewFallback();
            }
        }

        private void ConfigurarBluetoothViewFallback()
        {
            try
            {
                Debug.WriteLine("[MainPage] Configurando BluetoothView fallback");

                // Criar uma view de fallback simples
                var fallbackView = new ContentView
                {
                    Content = new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = "Bluetooth View não disponível",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                FontSize = 16
                            },
                            new Label
                            {
                                Text = "Verificque as configurações e tente novamente",
                                HorizontalOptions = LayoutOptions.Center,
                                FontSize = 12,
                                TextColor = Colors.Gray
                            }
                        },
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };

                bluetoothViewContent.Content = fallbackView;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainPage] ERRO crítico no fallback: {ex.Message}");
            }
        }
    }
}