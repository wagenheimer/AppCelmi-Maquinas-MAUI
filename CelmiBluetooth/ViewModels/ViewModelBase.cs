using CommunityToolkit.Mvvm.ComponentModel;
using LocalizationResourceManager.Maui;
using System.ComponentModel;

namespace CelmiBluetooth.ViewModels
{
    /// <summary>
    /// Classe base para ViewModels, fornecendo funcionalidades comuns como o gerenciamento de recursos de localização.
    /// Herda de ObservableObject para suportar a notificação de alteração de propriedades.
    /// </summary>
    public abstract partial class ViewModelBase : ObservableObject, IDisposable
    {
        /// <summary>
        /// Gerenciador de recursos de localização, usado para obter strings traduzidas.
        /// </summary>
        protected readonly ILocalizationResourceManager ResourceManager;

        /// <summary>
        /// Referência para o evento subscrito para permitir unsubscribe.
        /// </summary>
        private readonly INotifyPropertyChanged? _resourceManagerNotify;
        private bool _disposed = false;

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
                _resourceManagerNotify = notifyPropertyChanged;
                _resourceManagerNotify.PropertyChanged += ResourceManager_PropertyChanged;
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

        /// <summary>
        /// Libera todos os recursos utilizados pelo ViewModel.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera os recursos gerenciados e não-gerenciados utilizados pelo ViewModel.
        /// </summary>
        /// <param name="disposing">true se chamado de Dispose(); false se chamado do finalizador.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Unsubscribe do evento do ResourceManager
                if (_resourceManagerNotify != null)
                {
                    _resourceManagerNotify.PropertyChanged -= ResourceManager_PropertyChanged;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizador para garantir que os recursos sejam liberados mesmo se Dispose não for chamado.
        /// </summary>
        ~ViewModelBase()
        {
            Dispose(false);
        }
    }
}
