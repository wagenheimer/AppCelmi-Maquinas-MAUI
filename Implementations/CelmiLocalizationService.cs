using CommunityToolkit.Mvvm.ComponentModel;
using LocalizationResourceManager.Maui;
using Microsoft.Maui.ApplicationModel;
using AppCelmiPecuaria.Services;
using System.ComponentModel;

namespace AppCelmiPecuaria.Implementations
{
    /// <summary>
    /// Servi�o respons�vel por gerenciar textos localizados e informa��es de vers�o do app.
    /// </summary>
    public partial class CelmiLocalizationService : ObservableObject, ICelmiLocalizationService
    {
        private readonly ILocalizationResourceManager _resourceManager;

        [ObservableProperty]
        private string appVersionText = string.Empty;

        public CelmiLocalizationService(ILocalizationResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            if (_resourceManager is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += ResourceManager_PropertyChanged!;
            }
            UpdateLocalizedProperties();
        }

        public void UpdateLocalizedProperties()
        {
            AppVersionText = $"{_resourceManager["Vers�o"]} {AppInfo.Current.VersionString}";
        }

        private void ResourceManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ILocalizationResourceManager.CurrentCulture))
            {
                UpdateLocalizedProperties();
            }
        }
    }
}