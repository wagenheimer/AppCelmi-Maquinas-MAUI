using CelmiBluetooth.Services.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using System.Collections.ObjectModel;
using CelmiBluetooth.Models;

namespace AppCelmiMaquinas.ViewModel
{
    /// <summary>
    /// ViewModel para a tela de configura��es do aplicativo.
    /// </summary>
    public partial class ConfiguracaoViewModel : ObservableObject
    {
        private readonly ICelmiBluetoothConfigurationService _configService;

        /// <summary>
        /// Configura��es da empresa.
        /// </summary>
        [ObservableProperty]
        private CompanyConfiguration _companyConfiguration;

        /// <summary>
        /// Configura��es de relat�rio.
        /// </summary>
        [ObservableProperty]
        private ReportConfiguration _reportConfiguration;

        public ConfiguracaoViewModel(ICelmiBluetoothConfigurationService configService)
        {
            _configService = configService;

            // Carrega as se��es de configura��o
            _companyConfiguration = _configService.GetConfiguration<CompanyConfiguration>();
            _reportConfiguration = _configService.GetConfiguration<ReportConfiguration>();
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            // O salvamento agora � autom�tico ao alterar qualquer propriedade,
            // mas podemos manter um bot�o de salvar para for�ar a persist�ncia se necess�rio.
            await _configService.SaveAsync();
        }
    }
}