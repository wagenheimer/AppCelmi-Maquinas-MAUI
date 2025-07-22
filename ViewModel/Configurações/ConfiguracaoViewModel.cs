using CelmiBluetooth.Services.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using System.Collections.ObjectModel;
using CelmiBluetooth.Models;

namespace AppCelmiMaquinas.ViewModel
{
    /// <summary>
    /// ViewModel para a tela de configurações do aplicativo.
    /// </summary>
    public partial class ConfiguracaoViewModel : ObservableObject
    {
        private readonly ICelmiBluetoothConfigurationService _configService;

        /// <summary>
        /// Configurações da empresa.
        /// </summary>
        [ObservableProperty]
        private CompanyConfiguration _companyConfiguration;

        /// <summary>
        /// Configurações de relatório.
        /// </summary>
        [ObservableProperty]
        private ReportConfiguration _reportConfiguration;

        public ConfiguracaoViewModel(ICelmiBluetoothConfigurationService configService)
        {
            _configService = configService;

            // Carrega as seções de configuração
            _companyConfiguration = _configService.GetConfiguration<CompanyConfiguration>();
            _reportConfiguration = _configService.GetConfiguration<ReportConfiguration>();
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            // O salvamento agora é automático ao alterar qualquer propriedade,
            // mas podemos manter um botão de salvar para forçar a persistência se necessário.
            await _configService.SaveAsync();
        }
    }
}