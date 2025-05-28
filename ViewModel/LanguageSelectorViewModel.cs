using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;

namespace AppCelmiPecuaria.ViewModel
{
    /// <summary>
    /// Representa um idioma disponível para seleção, incluindo o caminho da imagem da bandeira.
    /// </summary>
    public partial class LanguageInfo : ObservableObject
    {
        [ObservableProperty]
        private string cultureName;

        [ObservableProperty]
        private string displayName;

        public string ImagePath => $"lang_{CultureName?.ToLower().Replace("-", "_")}.png";

        [ObservableProperty]
        private bool isSelected;
    }

    /// <summary>
    /// ViewModel para seleção de idioma, usando imagens em vez de ComboBox.
    /// </summary>
    public partial class LanguageSelectorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private LanguageInfo selectedLanguage;

        public ObservableCollection<LanguageInfo> AvailableLanguages { get; }

        public IRelayCommand<LanguageInfo> SelectLanguageCommand { get; }

        public LanguageSelectorViewModel(ILocalizationResourceManager resourceManager)
            : base(resourceManager)
        {
            AvailableLanguages = new ObservableCollection<LanguageInfo>(
                new[]
                {
                    new LanguageInfo { CultureName = "en-US", DisplayName = "English" },
                    new LanguageInfo { CultureName = "es-ES", DisplayName = "Español" },
                    new LanguageInfo { CultureName = "pt-BR", DisplayName = "Português (Brasil)" }
                });

            var currentCultureName = ResourceManager.CurrentCulture.Name;
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureName == currentCultureName)
                               ?? AvailableLanguages.FirstOrDefault(l => l.CultureName == "pt-BR");

            SelectLanguageCommand = new RelayCommand<LanguageInfo>(OnSelectLanguage);
            UpdateSelection();
        }

        partial void OnSelectedLanguageChanged(LanguageInfo value)
        {
            if (value != null)
            {
                ResourceManager.CurrentCulture = new CultureInfo(value.CultureName);
                UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            foreach (var lang in AvailableLanguages)
                lang.IsSelected = (lang == SelectedLanguage);
        }

        private void OnSelectLanguage(LanguageInfo lang)
        {
            if (lang != null && lang != SelectedLanguage)
                SelectedLanguage = lang;
        }
    }
}