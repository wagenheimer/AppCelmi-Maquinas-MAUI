using CelmiBluetooth.Models;
using CelmiBluetooth.Services.Configuration;

using System.Collections.ObjectModel;

namespace AppCelmiMaquinas.Implementations
{
    /// <summary>
    /// Fornece as configurações personalizadas para o aplicativo AppCelmiMaquinas,
    /// incluindo os campos de relatório padrão.
    /// </summary>
    public class AppCelmiConfiguration : CelmiBluetoothConfiguration
    {
        public AppCelmiConfiguration()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// Define os valores padrão para as configurações, incluindo os campos personalizados.
        /// </summary>
        private void SetDefaultValues()
        {
            // Define os campos personalizados padrão para este aplicativo
            ReportConfiguration.CustomFields = new ObservableCollection<CustomField>
            {
                new CustomField { Title = "Cliente", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Cliente" },
                new CustomField { Title = "Fazenda/Propriedade", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Fazenda" },
                new CustomField { Title = "Cidade/Localização", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Cidade" },
                new CustomField { Title = "Modelo", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Modelo" },
                new CustomField { Title = "Chassi", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Chassi" },
                new CustomField { Title = "Horímetro", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Horimetro" },
                new CustomField { Title = "Responsável", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Responsavel" }
            };
        }
    }
}