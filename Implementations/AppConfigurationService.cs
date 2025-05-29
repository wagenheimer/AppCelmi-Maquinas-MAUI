using System.Globalization;
using System.Text.Json;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using AppCelmiPecuaria.Models;
using AppCelmiPecuaria.Services;
using Shiny.Stores;

namespace AppCelmiPecuaria.Implementations
{
    /// <summary>
    /// Serviço de configuração persistente usando Shiny's IKeyValueStore.
    /// Responsável por salvar e carregar configurações do aplicativo.
    /// </summary>
    public partial class AppConfigurationService : ObservableObject, IAppConfigurationService
    {
        private const string CULTURE_KEY = "CurrentCulture";
        private const string DEFAULT_CULTURE = "pt-BR";
        private const string CUSTOM_FIELDS_KEY = "CustomFields";
        private readonly IKeyValueStore _store;

        /// <summary>
        /// Obtém ou define o identificador da cultura atual usado para localização.
        /// O valor é automaticamente persistido quando alterado.
        /// </summary>
        [ObservableProperty]
        private string _currentCulture = DEFAULT_CULTURE;

        /// <summary>
        /// Lista de campos personalizados configurados
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CustomField> customFields = new();

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="AppConfigurationService"/>.
        /// </summary>
        /// <param name="store">O armazenamento chave-valor usado para persistir as configurações.</param>
        public AppConfigurationService(IKeyValueStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Carrega a configuração persistida do armazenamento.
        /// Se nenhuma configuração existir, os valores padrão são usados.
        /// </summary>
        public void Load()
        {
            CurrentCulture = _store.Get(typeof(string), CULTURE_KEY) as string ?? DEFAULT_CULTURE;
            LoadCustomFields();
        }

        /// <summary>
        /// Salva a configuração atual no armazenamento.
        /// </summary>
        public void Save()
        {
            _store.Set(CULTURE_KEY, CurrentCulture);
            SaveCustomFields();
        }

        /// <summary>
        /// Chamado quando a propriedade CurrentCulture é alterada.
        /// Salva automaticamente o novo valor no armazenamento persistente.
        /// </summary>
        /// <param name="value">O novo valor da cultura</param>
        partial void OnCurrentCultureChanged(string value) => Save();

        /// <summary>
        /// Adiciona um novo campo personalizado
        /// </summary>
        /// <param name="field">Campo a ser adicionado</param>
        public void AddCustomField(CustomField field)
        {
            field.DisplayOrder = CustomFields.Count;
            CustomFields.Add(field);
            Save();
        }

        /// <summary>
        /// Remove um campo personalizado
        /// </summary>
        /// <param name="field">Campo a ser removido</param>
        public void RemoveCustomField(CustomField field)
        {
            CustomFields.Remove(field);
            ReorderCustomFields();
            Save();
        }

        /// <summary>
        /// Move um campo personalizado para cima na ordem
        /// </summary>
        /// <param name="field">Campo a ser movido</param>
        public void MoveCustomFieldUp(CustomField field)
        {
            var index = CustomFields.IndexOf(field);
            if (index > 0)
            {
                CustomFields.Move(index, index - 1);
                ReorderCustomFields();
            }
            Save();
        }

        /// <summary>
        /// Move um campo personalizado para baixo na ordem
        /// </summary>
        /// <param name="field">Campo a ser movido</param>
        public void MoveCustomFieldDown(CustomField field)
        {
            var index = CustomFields.IndexOf(field);
            if (index < CustomFields.Count - 1)
            {
                CustomFields.Move(index, index + 1);
                ReorderCustomFields();
            }
        }

        private void LoadCustomFields()
        {
            var json = _store.Get(typeof(string), CUSTOM_FIELDS_KEY) as string;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var fields = JsonSerializer.Deserialize<List<CustomField>>(json);
                    if (fields != null)
                    {
                        CustomFields = new ObservableCollection<CustomField>(fields.OrderBy(f => f.DisplayOrder));
                    }
                }
                catch (JsonException)
                {
                    CustomFields.Clear();
                }
            }
        }

        private void SaveCustomFields()
        {
            var json = JsonSerializer.Serialize(CustomFields);
            _store.Set(CUSTOM_FIELDS_KEY, json);
        }

        private void ReorderCustomFields()
        {
            for (var i = 0; i < CustomFields.Count; i++)
            {
                CustomFields[i].DisplayOrder = i;
            }
            SaveCustomFields();
        }
    }
}