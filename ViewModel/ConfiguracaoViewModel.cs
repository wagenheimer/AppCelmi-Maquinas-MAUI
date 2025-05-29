using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using AppCelmiPecuaria.Models;
using AppCelmiPecuaria.Services;

namespace AppCelmiPecuaria.ViewModel
{
    public partial class ConfiguracaoViewModel : ViewModelBase
    {
        private readonly IAppConfigurationService _appConfig;

        public LanguageSelectorViewModel LanguageSelectorViewModel { get; }

        [ObservableProperty]
        private CustomField? selectedCustomField;

        public IAppConfigurationService AppConfig => _appConfig;

        public ConfiguracaoViewModel(ILocalizationResourceManager resourceManager, 
            LanguageSelectorViewModel languageSelectorViewModel,
            IAppConfigurationService appConfig)
            : base(resourceManager)
        {
            _appConfig = appConfig;
            LanguageSelectorViewModel = languageSelectorViewModel;

            // Carregar as configurações ao inicializar
            _appConfig.Load();
        }

        [RelayCommand]
        private async Task AddCustomField()
        {
            if (Application.Current?.MainPage == null)
                return;

            var result = await Application.Current.MainPage.DisplayPromptAsync(
                ResourceManager["NovoTitulo"],
                ResourceManager["InserirTituloCampo"],
                ResourceManager["Confirmar"],
                ResourceManager["Cancelar"]);

            if (!string.IsNullOrWhiteSpace(result))
            {
                var field = new CustomField
                {
                    Title = result,
                    Description = string.Empty,
                    FieldType = CustomFieldType.Text
                };

                _appConfig.AddCustomField(field);
                SelectedCustomField = field;
            }
        }

        [RelayCommand]
        private void RemoveCustomField(CustomField field)
        {
            _appConfig.RemoveCustomField(field);
            if (SelectedCustomField == field)
            {
                SelectedCustomField = null;
            }
        }

        [RelayCommand]
        private async Task EditCustomField(CustomField field)
        {
            if (Application.Current?.MainPage == null)
                return;

            var result = await Application.Current.MainPage.DisplayPromptAsync(
                ResourceManager["EditarTitulo"],
                ResourceManager["EditarTituloCampo"],
                ResourceManager["Confirmar"],
                ResourceManager["Cancelar"],
                initialValue: field.Title);

            if (!string.IsNullOrWhiteSpace(result))
            {
                field.Title = result;
                field.LastModified = DateTime.Now;
                _appConfig.Save();
            }
        }

        [RelayCommand]
        private void MoveCustomFieldUp(CustomField field)
        {
            _appConfig.MoveCustomFieldUp(field);
        }

        [RelayCommand]
        private void MoveCustomFieldDown(CustomField field)
        {
            _appConfig.MoveCustomFieldDown(field);
        }

        [RelayCommand]
        private async Task ConfigureFieldOptions(CustomField field)
        {
            if (field.FieldType != CustomFieldType.SingleChoice && field.FieldType != CustomFieldType.MultiChoice)
            {
                return;
            }

            if (Application.Current?.MainPage == null)
                return;

            var currentOptions = string.Join("\n", field.Options);
            var result = await Application.Current.MainPage.DisplayPromptAsync(
                ResourceManager["ConfigurarOpcoes"],
                ResourceManager["InserirOpcoesPorLinha"],
                ResourceManager["Confirmar"],
                ResourceManager["Cancelar"],
                initialValue: currentOptions,
                maxLength: 1000,
                keyboard: Keyboard.Text);

            if (result != null)
            {
                field.Options.Clear();
                var options = result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(o => o.Trim())
                                  .Where(o => !string.IsNullOrWhiteSpace(o));

                foreach (var option in options)
                {
                    field.Options.Add(option);
                }

                field.LastModified = DateTime.Now;
                _appConfig.Save();
            }
        }

        protected override void UpdateLocalizedProperties()
        {
            OnPropertyChanged(nameof(LanguageSelectorViewModel));
        }
    }
}
