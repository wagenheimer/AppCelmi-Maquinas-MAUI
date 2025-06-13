using CommunityToolkit.Mvvm.ComponentModel;
using LocalizationResourceManager.Maui;
using System.ComponentModel;

namespace AppCelmiMaquinas.ViewModel
{
    /// <summary>
    /// Classe base para ViewModels, fornecendo funcionalidades comuns como o gerenciamento de recursos de localização.
    /// Herda de ObservableObject para suportar a notificação de alteração de propriedades.
    /// </summary>
    public abstract partial class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Gerenciador de recursos de localização, usado para obter strings traduzidas.
        /// </summary>
        protected readonly ILocalizationResourceManager ResourceManager;

        /// <summary>
        /// Construtor da ViewModelBase.
        /// </summary>
        /// <param name="resourceManager">Instância do gerenciador de recursos de localização.</param>
        protected ViewModelBase(ILocalizationResourceManager resourceManager)
        {
            ResourceManager = resourceManager;
            // Inscreve-se no evento PropertyChanged do ResourceManager para atualizar as propriedades localizadas
            // quando a cultura atual é alterada.
            if (ResourceManager is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += ResourceManager_PropertyChanged;
            }
        }

        /// <summary>
        /// Manipulador de evento para a alteração de propriedade do ResourceManager.
        /// Chamado quando uma propriedade do ResourceManager é alterada, como a CurrentCulture.
        /// </summary>
        /// <param name="sender">O objeto que disparou o evento (ResourceManager).</param>
        /// <param name="e">Argumentos do evento, contendo o nome da propriedade alterada.</param>
        private void ResourceManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Verifica se a propriedade alterada é a CurrentCulture.
            if (e.PropertyName == nameof(ILocalizationResourceManager.CurrentCulture))
            {
                // Se a cultura mudou, chama o método para atualizar as propriedades que dependem da localização.
                UpdateLocalizedProperties();
            }
        }

        /// <summary>
        /// Atualiza as propriedades localizadas do ViewModel.
        /// Este método é virtual e pode ser sobrescrito por classes derivadas
        /// para atualizar strings ou outros elementos que dependem da cultura atual.
        /// A implementação padrão é vazia.
        /// </summary>
        protected virtual void UpdateLocalizedProperties()
        {
            // Implementação padrão, pode ser sobrescrita nas classes derivadas.
            // Este método é chamado quando a cultura da aplicação é alterada.
        }
    }
}
