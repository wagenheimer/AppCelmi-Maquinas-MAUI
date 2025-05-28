using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using AppCelmiPecuaria.Services;
using Shiny.Stores;

namespace AppCelmiPecuaria.Implementations
{
    /// <summary>
    /// Servi�o de configura��o persistente usando Shiny's IKeyValueStore.
    /// Respons�vel por salvar e carregar configura��es do aplicativo.
    /// </summary>
    public partial class AppConfigurationService : ObservableObject, IAppConfigurationService
    {
        private const string CULTURE_KEY = "CurrentCulture";
        private const string DEFAULT_CULTURE = "pt-BR";
        private readonly IKeyValueStore _store;

        /// <summary>
        /// Obt�m ou define o identificador da cultura atual usado para localiza��o.
        /// O valor � automaticamente persistido quando alterado.
        /// </summary>
        [ObservableProperty]
        private string _currentCulture = DEFAULT_CULTURE;

        /// <summary>
        /// Inicializa uma nova inst�ncia da classe <see cref="AppConfigurationService"/>.
        /// </summary>
        /// <param name="store">O armazenamento chave-valor usado para persistir as configura��es.</param>
        public AppConfigurationService(IKeyValueStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Carrega a configura��o persistida do armazenamento.
        /// Se nenhuma configura��o existir, os valores padr�o s�o usados.
        /// </summary>
        public void Load()
        {
            CurrentCulture = _store.Get(typeof(string), CULTURE_KEY) as string ?? DEFAULT_CULTURE;
        }

        /// <summary>
        /// Salva a configura��o atual no armazenamento.
        /// </summary>
        public void Save()
        {
            _store.Set(CULTURE_KEY, CurrentCulture);
        }

        /// <summary>
        /// Chamado quando a propriedade CurrentCulture � alterada.
        /// Salva automaticamente o novo valor no armazenamento persistente.
        /// </summary>
        /// <param name="value">O novo valor da cultura</param>
        partial void OnCurrentCultureChanged(string value) => Save();
    }
}