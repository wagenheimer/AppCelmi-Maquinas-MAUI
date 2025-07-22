using CelmiBluetooth.Models;
using CelmiBluetooth.Services.Configuration;

using System.Collections.ObjectModel;

namespace AppCelmiMaquinas.Implementations
{
    /// <summary>
    /// Fornece as configura��es personalizadas para o aplicativo AppCelmiMaquinas,
    /// incluindo os campos de relat�rio padr�o.
    /// </summary>
    public class AppCelmiConfiguration : CelmiBluetoothConfiguration
    {
        public AppCelmiConfiguration()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// Define os valores padr�o para as configura��es, incluindo os campos personalizados.
        /// </summary>
        private void SetDefaultValues()
        {
            // Define os campos personalizados padr�o para este aplicativo
            ReportConfiguration.CustomFields = new ObservableCollection<CustomField>
            {
                new CustomField { Title = "Cliente", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Cliente" },
                new CustomField { Title = "Fazenda/Propriedade", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Fazenda" },
                new CustomField { Title = "Cidade/Localiza��o", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Cidade" },
                new CustomField { Title = "Modelo", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Modelo" },
                new CustomField { Title = "Chassi", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Chassi" },
                new CustomField { Title = "Hor�metro", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Horimetro" },
                new CustomField { Title = "Respons�vel", IsDefault = true, IsEnabled = true, TranslationKey = "CampoPadrao_Responsavel" }
            };
        }
    }
}