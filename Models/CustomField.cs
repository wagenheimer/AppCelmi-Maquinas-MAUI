using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppCelmiPecuaria.Models
{
    /// <summary>
    /// Tipo do campo personalizado
    /// </summary>
    public enum CustomFieldType
    {
        /// <summary>
        /// Campo de texto livre
        /// </summary>
        Text,

        /// <summary>
        /// Campo num�rico
        /// </summary>
        Number,

        /// <summary>
        /// Campo de data
        /// </summary>
        Date,

        /// <summary>
        /// Campo de sele��o �nica
        /// </summary>
        SingleChoice,

        /// <summary>
        /// Campo de sele��o m�ltipla
        /// </summary>
        MultiChoice
    }

    /// <summary>
    /// Representa um campo personalizado no sistema
    /// </summary>
    public partial class CustomField : ObservableObject
    {
        /// <summary>
        /// Identificador �nico do campo
        /// </summary>
        [ObservableProperty]
        private string id = Guid.NewGuid().ToString();

        /// <summary>
        /// T�tulo do campo que ser� exibido para o usu�rio
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// Descri��o ou dica sobre o campo
        /// </summary>
        [ObservableProperty]
        private string description = string.Empty;

        /// <summary>
        /// Tipo do campo
        /// </summary>
        [ObservableProperty]
        private CustomFieldType fieldType = CustomFieldType.Text;

        /// <summary>
        /// Indica se o campo � obrigat�rio
        /// </summary>
        [ObservableProperty]
        private bool isRequired;

        /// <summary>
        /// Ordem de exibi��o do campo
        /// </summary>
        [ObservableProperty]
        private int displayOrder;

        /// <summary>
        /// Valor padr�o do campo
        /// </summary>
        [ObservableProperty]
        private string defaultValue = string.Empty;

        /// <summary>
        /// Op��es dispon�veis para campos de sele��o
        /// </summary>
        public ObservableCollection<string> Options { get; } = new();

        /// <summary>
        /// Define se o campo est� ativo e deve ser exibido
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;

        /// <summary>
        /// Express�o regular para valida��o de campos de texto
        /// </summary>
        [ObservableProperty]
        private string validationRegex = string.Empty;

        /// <summary>
        /// Mensagem de erro para valida��o
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Valor m�nimo para campos num�ricos
        /// </summary>
        [ObservableProperty]
        private double? minimumValue;

        /// <summary>
        /// Valor m�ximo para campos num�ricos
        /// </summary>
        [ObservableProperty]
        private double? maximumValue;

        /// <summary>
        /// Data de cria��o do campo
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.Now;

        /// <summary>
        /// �ltima modifica��o do campo
        /// </summary>
        [ObservableProperty]
        private DateTime lastModified = DateTime.Now;

        partial void OnFieldTypeChanged(CustomFieldType value)
        {
            // Limpa as op��es se o tipo n�o for de sele��o
            if (value != CustomFieldType.SingleChoice && value != CustomFieldType.MultiChoice)
            {
                Options.Clear();
            }
        }
    }
}