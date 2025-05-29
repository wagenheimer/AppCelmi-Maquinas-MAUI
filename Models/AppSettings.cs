using System.Collections.Generic;
using AppCelmiPecuaria.Models;

namespace AppCelmiPecuaria.Models
{
    /// <summary>
    /// Representa todas as configurações persistentes do aplicativo.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Cultura atual do aplicativo.
        /// </summary>
        public string CurrentCulture { get; set; } = "pt-BR";

        /// <summary>
        /// Lista de campos personalizados configurados.
        /// </summary>
        public List<CustomField> CustomFields { get; set; } = new();
    }
}
