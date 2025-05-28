using CommunityToolkit.Mvvm.ComponentModel;
using LocalizationResourceManager.Maui;

namespace AppCelmiPecuaria.ViewModel
{
    public partial class ConfiguracaoViewModel : ViewModelBase
    {
        public LanguageSelectorViewModel LanguageSelectorViewModel { get; }

        public ConfiguracaoViewModel(ILocalizationResourceManager resourceManager, LanguageSelectorViewModel languageSelectorViewModel)
            : base(resourceManager)
        {
            LanguageSelectorViewModel = languageSelectorViewModel;
            // Inicialize outras propriedades ou comandos aqui, se necessário
        }

        protected override void UpdateLocalizedProperties()
        {
            // Atualize quaisquer propriedades localizadas específicas para ConfiguracaoViewModel aqui
            // Ex: MyLocalizedProperty = ResourceManager.GetValue("MyConfigKey");
            OnPropertyChanged(nameof(LanguageSelectorViewModel)); // Notificar que o ViewModel filho pode precisar ser atualizado
        }
    }
}
