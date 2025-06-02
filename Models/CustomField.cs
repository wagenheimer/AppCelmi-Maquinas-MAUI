using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiPecuaria.Models
{
    /// <summary>
    /// Representa um campo personalizado (apenas t�tulo).
    /// </summary>
    public partial class CustomField : ObservableObject
    {
        /// <summary>
        /// Construtor padr�o necess�rio para serializa��o/deserializa��o.
        /// </summary>
        public CustomField()
        {
        }

        /// <summary>
        /// T�tulo do campo personalizado.
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;
    }
}