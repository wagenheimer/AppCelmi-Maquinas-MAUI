using System.Collections.Generic;
using System.Collections.ObjectModel;

using AppCelmiMaquinas.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AppCelmiMaquinas.Models
{
    /// <summary>
    /// Representa todas as configura��es persistentes do aplicativo.
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
        /// Endere�o da empresa.
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
        /// Lista de campos personalizados configurados.
        /// </summary>
        [ObservableProperty]
        ObservableCollection<CustomField> customFields = new();

        /// <summary>
        /// Exibe informa��es cross-plataformas P1xP4 e P1xP3.
        /// </summary>
        [ObservableProperty]
        bool informacaoCrossPlataformas = false;
    }
}
