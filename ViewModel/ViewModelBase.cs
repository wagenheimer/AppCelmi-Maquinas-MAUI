using CommunityToolkit.Mvvm.ComponentModel;
using LocalizationResourceManager.Maui;
using System.ComponentModel;

namespace AppCelmiMaquinas.ViewModel
{
    /// <summary>
    /// Classe base para ViewModels, fornecendo funcionalidades comuns como o gerenciamento de recursos de localiza��o.
    /// Herda de ObservableObject para suportar a notifica��o de altera��o de propriedades.
    /// </summary>
    public abstract partial class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Gerenciador de recursos de localiza��o, usado para obter strings traduzidas.
        /// </summary>
        protected readonly ILocalizationResourceManager ResourceManager;

        /// <summary>
        /// Construtor da ViewModelBase.
        /// </summary>
        /// <param name="resourceManager">Inst�ncia do gerenciador de recursos de localiza��o.</param>
        protected ViewModelBase(ILocalizationResourceManager resourceManager)
        {
            ResourceManager = resourceManager;
            // Inscreve-se no evento PropertyChanged do ResourceManager para atualizar as propriedades localizadas
            // quando a cultura atual � alterada.
            if (ResourceManager is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += ResourceManager_PropertyChanged;
            }
        }

        /// <summary>
        /// Manipulador de evento para a altera��o de propriedade do ResourceManager.
        /// Chamado quando uma propriedade do ResourceManager � alterada, como a CurrentCulture.
        /// </summary>
        /// <param name="sender">O objeto que disparou o evento (ResourceManager).</param>
        /// <param name="e">Argumentos do evento, contendo o nome da propriedade alterada.</param>
        private void ResourceManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Verifica se a propriedade alterada � a CurrentCulture.
            if (e.PropertyName == nameof(ILocalizationResourceManager.CurrentCulture))
            {
                // Se a cultura mudou, chama o m�todo para atualizar as propriedades que dependem da localiza��o.
                UpdateLocalizedProperties();
            }
        }

        /// <summary>
        /// Atualiza as propriedades localizadas do ViewModel.
        /// Este m�todo � virtual e pode ser sobrescrito por classes derivadas
        /// para atualizar strings ou outros elementos que dependem da cultura atual.
        /// A implementa��o padr�o � vazia.
        /// </summary>
        protected virtual void UpdateLocalizedProperties()
        {
            // Implementa��o padr�o, pode ser sobrescrita nas classes derivadas.
            // Este m�todo � chamado quando a cultura da aplica��o � alterada.
        }
    }
}
