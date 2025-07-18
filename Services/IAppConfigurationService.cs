using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AppCelmiMaquinas.Services
{
    public interface ICelmiLocalizationService : INotifyPropertyChanged
    {
        string AppVersionText { get; }
        void UpdateLocalizedProperties();
    }

}
