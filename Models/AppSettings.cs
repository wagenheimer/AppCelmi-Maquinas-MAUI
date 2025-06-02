using System.Collections.Generic;
using System.Collections.ObjectModel;

using AppCelmiPecuaria.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiPecuaria.Models
{
    /// <summary>
    /// Representa todas as configurações persistentes do aplicativo.
    /// </summary>
    public partial class AppSettings : ObservableObject
    {
        /// <summary>
        /// Cultura atual do aplicativo.
        /// </summary>
        [ObservableProperty]
        string currentCulture = "pt-BR";

        /// <summary>
        /// Nome da empresa.
        /// </summary>
        [ObservableProperty]
        string nomeEmpresa = string.Empty;

        /// <summary>
        /// Endereço da empresa.
        /// </summary>
        [ObservableProperty]
        string endereco = string.Empty;

        /// <summary>
        /// Email de contato da empresa.
        /// </summary>
        [ObservableProperty]
        string email = string.Empty;

        /// <summary>
        /// Telefone de contato da empresa.
        /// </summary>
        [ObservableProperty]
        string telefone = string.Empty;

        /// <summary>
        /// Logomarca da empresa (armazenada como array de bytes).
        /// </summary>
        [ObservableProperty]
        byte[]? logoMarca;

        /// <summary>
        /// Lista de campos personalizados configurados.
        /// </summary>
        [ObservableProperty]
        ObservableCollection<CustomField> customFields = new();
    }
}
