using System.Text.Json;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using AppCelmiPecuaria.Models;
using AppCelmiPecuaria.Services;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace AppCelmiPecuaria.Implementations
{
    /// <summary>
    /// Servi�o respons�vel por salvar e carregar configura��es persistentes do aplicativo usando Preferences do .NET MAUI.
    /// </summary>
    public partial class AppConfigurationService : ObservableObject
    {
        private const string SETTINGS_KEY = "AppSettings";


        [ObservableProperty]
        private AppSettings appSettings;

        private static readonly JsonSerializerOptions _jsonOptions;

        #region AppConfigurationService
        static AppConfigurationService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Inicializa uma nova inst�ncia do servi�o de configura��o.
        /// </summary>
        public AppConfigurationService()
        {
            AppSettings = new AppSettings(); // Inicializa com configura��es padr�o
            Load(); // Carrega as configura��es ao inicializar o servi�o
        }

        /// <summary>
        /// Carrega as configura��es persistidas do armazenamento.
        /// </summary>
        public void Load()
        {
            Debug.WriteLine("[AppConfigurationService] Iniciando Load().");
            var json = Preferences.Get(SETTINGS_KEY, null);

            if (!string.IsNullOrEmpty(json))
            {
                Debug.WriteLine($"[AppConfigurationService] JSON lido das Preferences: {json}");
                try
                {
                    AppSettings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
                    Debug.WriteLine($"[AppConfigurationService] Desserializa��o bem-sucedida. _settings.CurrentCulture: {AppSettings.CurrentCulture}");
                    Debug.WriteLine($"[AppConfigurationService] _settings.CustomFields count: {(AppSettings.CustomFields?.Count ?? 0)}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AppConfigurationService] Erro ao deserializar configura��es: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"[AppConfigurationService] Inner exception: {ex.InnerException.Message}");
                    }
                    Debug.WriteLine($"[AppConfigurationService] JSON com erro: {json}");
                    AppSettings = new AppSettings(); // Reseta para configura��es padr�o em caso de erro
                }
            }
            else
            {
                Debug.WriteLine("[AppConfigurationService] Nenhuma configura��o encontrada para carregar (JSON vazio ou nulo).");
                AppSettings = new AppSettings(); // Garante que _settings seja inicializado
            }
        }

        /// <summary>
        /// Salva as configura��es atuais no armazenamento persistente.
        /// </summary>
        public void Save()
        {
            Debug.WriteLine("[AppConfigurationService] Iniciando Save().");
            Debug.WriteLine($"[AppConfigurationService] _settings preparado para salvar. CustomFields count: {AppSettings.CustomFields.Count}");

            var json = JsonSerializer.Serialize(AppSettings, _jsonOptions);
            Preferences.Set(SETTINGS_KEY, json);

            Debug.WriteLine($"[AppConfigurationService] Configura��es salvas nas Preferences: {json}");
        }
        #endregion

        #region Custom Fields        
        /// <summary>
        /// Adiciona um novo campo personalizado � lista e salva as configura��es.
        /// </summary>
        /// <param name="field">Campo a ser adicionado.</param>
        public void AddCustomField(CustomField field)
        {
            if (field == null)
            {
                Debug.WriteLine("[AppConfigurationService] Tentativa de adicionar um CustomField nulo. Ignorando.");
                return;
            }
            Debug.WriteLine($"[AppConfigurationService] Adicionando CustomField com Title: '{field.Title}'. Save() ser� chamado via CollectionChanged.");
            AppSettings.CustomFields.Add(field);
            Save(); // Garante que as altera��es sejam salvas imediatamente
        }

        /// <summary>
        /// Remove um campo personalizado da lista e salva as configura��es.
        /// </summary>
        /// <param name="field">Campo a ser removido.</param>
        public void RemoveCustomField(CustomField field)
        {
            if (field == null)
            {
                Debug.WriteLine("[AppConfigurationService] Tentativa de remover um CustomField nulo. Ignorando.");
                return;
            }
            Debug.WriteLine($"[AppConfigurationService] Removendo CustomField com Title: '{field.Title}'. Save() ser� chamado via CollectionChanged.");
            AppSettings.CustomFields.Remove(field);
            Save(); // Garante que as altera��es sejam salvas imediatamente
        }
        #endregion
    }
}