using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiPecuaria.Models
{
    /// <summary>
    /// Representa um campo personalizado (apenas título).
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
        /// Título do campo personalizado.
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;
    }
}