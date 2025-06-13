using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiMaquinas.Models
{
    /// <summary>
    /// Representa um campo personalizado ou padrão.
    /// </summary>
    public partial class CustomField : ObservableObject
    {
        /// <summary>
        /// Construtor padrão necessário para serialização/deserialização.
        /// </summary>
        public CustomField()
        {
        }

        /// <summary>
        /// Título do campo personalizado ou padrão.
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// Indica se o campo é padrão (não pode ser excluído).
        /// </summary>
        [ObservableProperty]
        private bool isDefault;

        /// <summary>
        /// Indica se o campo está habilitado ("Utiliza").
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;

        /// <summary>
        /// Chave de tradução para campos padrão (opcional).
        /// </summary>
        [ObservableProperty]
        private string? translationKey;
    }
}