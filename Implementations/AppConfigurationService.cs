using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
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
        private readonly IKeyValueStore _store;

        /// <summary>
        /// Obtém ou define o identificador da cultura atual usado para localização.
        /// O valor é automaticamente persistido quando alterado.
        /// </summary>
        [ObservableProperty]
        private string _currentCulture = DEFAULT_CULTURE;

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
        }

        /// <summary>
        /// Salva a configuração atual no armazenamento.
        /// </summary>
        public void Save()
        {
            _store.Set(CULTURE_KEY, CurrentCulture);
        }

        /// <summary>
        /// Chamado quando a propriedade CurrentCulture é alterada.
        /// Salva automaticamente o novo valor no armazenamento persistente.
        /// </summary>
        /// <param name="value">O novo valor da cultura</param>
        partial void OnCurrentCultureChanged(string value) => Save();
    }
}