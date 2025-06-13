using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiMaquinas.Models
{
    /// <summary>
    /// Representa um campo personalizado ou padr�o.
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
        /// T�tulo do campo personalizado ou padr�o.
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// Indica se o campo � padr�o (n�o pode ser exclu�do).
        /// </summary>
        [ObservableProperty]
        private bool isDefault;

        /// <summary>
        /// Indica se o campo est� habilitado ("Utiliza").
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;

        /// <summary>
        /// Chave de tradu��o para campos padr�o (opcional).
        /// </summary>
        [ObservableProperty]
        private string? translationKey;
    }
}