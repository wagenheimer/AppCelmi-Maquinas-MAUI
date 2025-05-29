using System.Collections.ObjectModel;
using System.ComponentModel;
using AppCelmiPecuaria.Models;

namespace AppCelmiPecuaria.Services
{
    public interface ICelmiLocalizationService : INotifyPropertyChanged
    {
        string AppVersionText { get; }
        void UpdateLocalizedProperties();
    }

    /// <summary>
    /// Interface para o serviço de configuração do aplicativo
    /// </summary>
    public interface IAppConfigurationService : INotifyPropertyChanged
    {
        /// <summary>
        /// Cultura atual do aplicativo
        /// </summary>
        string CurrentCulture { get; set; }

        /// <summary>
        /// Campos personalizados configurados
        /// </summary>
        ObservableCollection<CustomField> CustomFields { get; }

        /// <summary>
        /// Adiciona um novo campo personalizado
        /// </summary>
        /// <param name="field">Campo a ser adicionado</param>
        void AddCustomField(CustomField field);

        /// <summary>
        /// Remove um campo personalizado
        /// </summary>
        /// <param name="field">Campo a ser removido</param>
        void RemoveCustomField(CustomField field);

        /// <summary>
        /// Move um campo personalizado para cima na ordem
        /// </summary>
        /// <param name="field">Campo a ser movido</param>
        void MoveCustomFieldUp(CustomField field);

        /// <summary>
        /// Move um campo personalizado para baixo na ordem
        /// </summary>
        /// <param name="field">Campo a ser movido</param>
        void MoveCustomFieldDown(CustomField field);

        /// <summary>
        /// Salva as configurações
        /// </summary>
        void Save();

        /// <summary>
        /// Carrega as configurações
        /// </summary>
        void Load();
    }
}
