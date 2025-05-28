using System.ComponentModel;

namespace AppCelmiPecuaria.Services
{
    public interface ICelmiLocalizationService : INotifyPropertyChanged
    {
        string AppVersionText { get; }
        void UpdateLocalizedProperties();
    }

    public interface IAppConfigurationService : INotifyPropertyChanged
    {
        string CurrentCulture { get; set; }
        void Save();
        void Load();
    }
}
