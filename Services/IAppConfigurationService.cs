using System.Collections.ObjectModel;
using System.ComponentModel;
using AppCelmiMaquinas.Models;

namespace AppCelmiMaquinas.Services
{
    public interface ICelmiLocalizationService : INotifyPropertyChanged
    {
        string AppVersionText { get; }
        void UpdateLocalizedProperties();
    }

}
