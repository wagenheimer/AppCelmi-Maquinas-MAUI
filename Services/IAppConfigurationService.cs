using System.Collections.ObjectModel;
using System.ComponentModel;
using AppCelmiPecuaria.Models;

namespace AppCelmiPecuaria.Services
{
    public interface ICelmiLocalizationService : INotifyPropertyChanged
    {
        string AppVersionText { get; }
        void UpdateLocalizedProperties();
    }

}
