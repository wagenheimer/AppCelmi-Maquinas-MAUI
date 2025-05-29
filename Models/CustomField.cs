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
        /// Campo numérico
        /// </summary>
        Number,

        /// <summary>
        /// Campo de data
        /// </summary>
        Date,

        /// <summary>
        /// Campo de seleção única
        /// </summary>
        SingleChoice,

        /// <summary>
        /// Campo de seleção múltipla
        /// </summary>
        MultiChoice
    }

    /// <summary>
    /// Representa um campo personalizado no sistema
    /// </summary>
    public partial class CustomField : ObservableObject
    {
        /// <summary>
        /// Identificador único do campo
        /// </summary>
        [ObservableProperty]
        private string id = Guid.NewGuid().ToString();

        /// <summary>
        /// Título do campo que será exibido para o usuário
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// Descrição ou dica sobre o campo
        /// </summary>
        [ObservableProperty]
        private string description = string.Empty;

        /// <summary>
        /// Tipo do campo
        /// </summary>
        [ObservableProperty]
        private CustomFieldType fieldType = CustomFieldType.Text;

        /// <summary>
        /// Indica se o campo é obrigatório
        /// </summary>
        [ObservableProperty]
        private bool isRequired;

        /// <summary>
        /// Ordem de exibição do campo
        /// </summary>
        [ObservableProperty]
        private int displayOrder;

        /// <summary>
        /// Valor padrão do campo
        /// </summary>
        [ObservableProperty]
        private string defaultValue = string.Empty;

        /// <summary>
        /// Opções disponíveis para campos de seleção
        /// </summary>
        public ObservableCollection<string> Options { get; } = new();

        /// <summary>
        /// Define se o campo está ativo e deve ser exibido
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;

        /// <summary>
        /// Expressão regular para validação de campos de texto
        /// </summary>
        [ObservableProperty]
        private string validationRegex = string.Empty;

        /// <summary>
        /// Mensagem de erro para validação
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Valor mínimo para campos numéricos
        /// </summary>
        [ObservableProperty]
        private double? minimumValue;

        /// <summary>
        /// Valor máximo para campos numéricos
        /// </summary>
        [ObservableProperty]
        private double? maximumValue;

        /// <summary>
        /// Data de criação do campo
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.Now;

        /// <summary>
        /// Última modificação do campo
        /// </summary>
        [ObservableProperty]
        private DateTime lastModified = DateTime.Now;

        partial void OnFieldTypeChanged(CustomFieldType value)
        {
            // Limpa as opções se o tipo não for de seleção
            if (value != CustomFieldType.SingleChoice && value != CustomFieldType.MultiChoice)
            {
                Options.Clear();
            }
        }
    }
}