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
    /// Serviço responsável por salvar e carregar configurações persistentes do aplicativo usando Preferences do .NET MAUI.
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
        /// Inicializa uma nova instância do serviço de configuração.
        /// </summary>
        public AppConfigurationService()
        {
            AppSettings = new AppSettings(); // Inicializa com configurações padrão
            Load(); // Carrega as configurações ao inicializar o serviço
        }

        /// <summary>
        /// Carrega as configurações persistidas do armazenamento.
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
                    Debug.WriteLine($"[AppConfigurationService] Desserialização bem-sucedida. _settings.CurrentCulture: {AppSettings.CurrentCulture}");
                    Debug.WriteLine($"[AppConfigurationService] _settings.CustomFields count: {(AppSettings.CustomFields?.Count ?? 0)}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AppConfigurationService] Erro ao deserializar configurações: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"[AppConfigurationService] Inner exception: {ex.InnerException.Message}");
                    }
                    Debug.WriteLine($"[AppConfigurationService] JSON com erro: {json}");
                    AppSettings = new AppSettings(); // Reseta para configurações padrão em caso de erro
                }
            }
            else
            {
                Debug.WriteLine("[AppConfigurationService] Nenhuma configuração encontrada para carregar (JSON vazio ou nulo).");
                AppSettings = new AppSettings(); // Garante que _settings seja inicializado
            }
        }

        /// <summary>
        /// Salva as configurações atuais no armazenamento persistente.
        /// </summary>
        public void Save()
        {
            Debug.WriteLine("[AppConfigurationService] Iniciando Save().");
            Debug.WriteLine($"[AppConfigurationService] _settings preparado para salvar. CustomFields count: {AppSettings.CustomFields.Count}");

            var json = JsonSerializer.Serialize(AppSettings, _jsonOptions);
            Preferences.Set(SETTINGS_KEY, json);

            Debug.WriteLine($"[AppConfigurationService] Configurações salvas nas Preferences: {json}");
        }
        #endregion

        #region Custom Fields        
        /// <summary>
        /// Adiciona um novo campo personalizado à lista e salva as configurações.
        /// </summary>
        /// <param name="field">Campo a ser adicionado.</param>
        public void AddCustomField(CustomField field)
        {
            if (field == null)
            {
                Debug.WriteLine("[AppConfigurationService] Tentativa de adicionar um CustomField nulo. Ignorando.");
                return;
            }
            Debug.WriteLine($"[AppConfigurationService] Adicionando CustomField com Title: '{field.Title}'. Save() será chamado via CollectionChanged.");
            AppSettings.CustomFields.Add(field);
            Save(); // Garante que as alterações sejam salvas imediatamente
        }

        /// <summary>
        /// Remove um campo personalizado da lista e salva as configurações.
        /// </summary>
        /// <param name="field">Campo a ser removido.</param>
        public void RemoveCustomField(CustomField field)
        {
            if (field == null)
            {
                Debug.WriteLine("[AppConfigurationService] Tentativa de remover um CustomField nulo. Ignorando.");
                return;
            }
            Debug.WriteLine($"[AppConfigurationService] Removendo CustomField com Title: '{field.Title}'. Save() será chamado via CollectionChanged.");
            AppSettings.CustomFields.Remove(field);
            Save(); // Garante que as alterações sejam salvas imediatamente
        }
        #endregion
    }
}