using System.Text.Json;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using AppCelmiPecuaria.Models;
using AppCelmiPecuaria.Services;
using Microsoft.Maui.Storage;

namespace AppCelmiPecuaria.Implementations
{
    /// <summary>
    /// Servi�o respons�vel por salvar e carregar configura��es persistentes do aplicativo usando Preferences do .NET MAUI.
    /// </summary>
    public partial class AppConfigurationService : ObservableObject, IAppConfigurationService
    {
        private const string SETTINGS_KEY = "AppSettings";
        private AppSettings _settings = new();
        private readonly ObservableCollection<CustomField> _customFields;

        /// <summary>
        /// Cultura atual do aplicativo. Sempre persistida ao ser alterada.
        /// </summary>
        [ObservableProperty]
        private string currentCulture;

        /// <summary>
        /// Chamado automaticamente quando <see cref="CurrentCulture"/> � alterado.
        /// Persiste a configura��o.
        /// </summary>
        /// <param name="value">Novo valor da cultura.</param>
        partial void OnCurrentCultureChanged(string value)
        {
            Save();
        }

        /// <summary>
        /// Lista de campos personalizados configurados.
        /// </summary>
        public ObservableCollection<CustomField> CustomFields => _customFields;

        /// <summary>
        /// Inicializa uma nova inst�ncia do servi�o de configura��o.
        /// </summary>
        public AppConfigurationService()
        {
            currentCulture = _settings.CurrentCulture;
            _customFields = new ObservableCollection<CustomField>(_settings.CustomFields);
            _customFields.CollectionChanged += (s, e) => Save();
        }

        /// <summary>
        /// Carrega as configura��es persistidas do armazenamento.
        /// </summary>
        public void Load()
        {
            var json = Preferences.Get(SETTINGS_KEY, null);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    _settings = new AppSettings();
                }
            }
            // Atualiza propriedades p�blicas
            CurrentCulture = _settings.CurrentCulture;
            _customFields.Clear();
            foreach (var field in _settings.CustomFields.OrderBy(f => f.DisplayOrder))
                _customFields.Add(field);
        }

        /// <summary>
        /// Salva as configura��es atuais no armazenamento persistente.
        /// </summary>
        public void Save()
        {
            _settings.CurrentCulture = CurrentCulture;
            _settings.CustomFields = _customFields.ToList();
            var json = JsonSerializer.Serialize(_settings);
            Preferences.Set(SETTINGS_KEY, json);
        }

        /// <summary>
        /// Adiciona um novo campo personalizado � lista e salva as configura��es.
        /// </summary>
        /// <param name="field">Campo a ser adicionado.</param>
        public void AddCustomField(CustomField field)
        {
            field.DisplayOrder = _customFields.Count;
            _customFields.Add(field);
            Save();
        }

        /// <summary>
        /// Remove um campo personalizado da lista e salva as configura��es.
        /// </summary>
        /// <param name="field">Campo a ser removido.</param>
        public void RemoveCustomField(CustomField field)
        {
            _customFields.Remove(field);
            ReorderCustomFields();
            Save();
        }

        /// <summary>
        /// Move um campo personalizado para cima na ordem de exibi��o e salva as configura��es.
        /// </summary>
        /// <param name="field">Campo a ser movido.</param>
        public void MoveCustomFieldUp(CustomField field)
        {
            var index = _customFields.IndexOf(field);
            if (index > 0)
            {
                _customFields.Move(index, index - 1);
                ReorderCustomFields();
                Save();
            }
        }

        /// <summary>
        /// Move um campo personalizado para baixo na ordem de exibi��o e salva as configura��es.
        /// </summary>
        /// <param name="field">Campo a ser movido.</param>
        public void MoveCustomFieldDown(CustomField field)
        {
            var index = _customFields.IndexOf(field);
            if (index < _customFields.Count - 1)
            {
                _customFields.Move(index, index + 1);
                ReorderCustomFields();
                Save();
            }
        }

        /// <summary>
        /// Atualiza a ordem de exibi��o dos campos personalizados.
        /// </summary>
        private void ReorderCustomFields()
        {
            for (var i = 0; i < _customFields.Count; i++)
                _customFields[i].DisplayOrder = i;
        }
    }
}