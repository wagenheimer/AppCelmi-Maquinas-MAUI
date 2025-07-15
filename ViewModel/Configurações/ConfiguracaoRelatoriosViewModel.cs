using AppCelmiMaquinas.Implementations;
using AppCelmiMaquinas.Models;
using AppCelmiMaquinas.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using LocalizationResourceManager.Maui;

using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Application = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Controls;
using CelmiBluetooth.ViewModels;

namespace AppCelmiMaquinas.ViewModel
{
    public partial class ConfiguracaoRelatoriosViewModel : ViewModelBase
    {
        private const string DefaultLogoPath = "Resources/Images/logo.png";
        private readonly string _logoFilePath = Path.Combine(FileSystem.AppDataDirectory, "logo.png");

        [ObservableProperty]
        AppConfigurationService appConfigurationService;

        [ObservableProperty]
        CustomField? _selectedCustomField;

        public ConfiguracaoRelatoriosViewModel(ILocalizationResourceManager resourceManager, AppConfigurationService appConfigurationService)
            : base(resourceManager)
        {
            Debug.WriteLine("[RelatorioCamposPersonalizadosViewModel] Construtor chamado.");
            AppConfigurationService = appConfigurationService;
            Debug.WriteLine($"[RelatorioCamposPersonalizadosViewModel] CustomFields count after init: {AppConfigurationService.AppSettings.CustomFields.Count}");

            // Copia logo padrão se não existir logo do usuário
            if (!File.Exists(_logoFilePath))
                CopyDefaultLogo();

            // Escuta mudanças na coleção para atualizar as listas filtradas
            AppConfigurationService.AppSettings.CustomFields.CollectionChanged += CustomFields_CollectionChanged;
        }

        public ImageSource LogoImageSource => File.Exists(_logoFilePath)
                ? ImageSource.FromFile(_logoFilePath)
                : ImageSource.FromFile(DefaultLogoPath);

        public IEnumerable<CustomField> DefaultFields => AppConfigurationService?.AppSettings.CustomFields.Where(f => f.IsDefault) ?? Enumerable.Empty<CustomField>();
        public IEnumerable<CustomField> CustomFieldsList => AppConfigurationService?.AppSettings.CustomFields.Where(f => !f.IsDefault) ?? Enumerable.Empty<CustomField>();

        private void CustomFields_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(DefaultFields));
            OnPropertyChanged(nameof(CustomFieldsList));

            UpdateMoveCommandsState();
         
        }

        void UpdateMoveCommandsState()
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                MoveCustomFieldUpCommand.NotifyCanExecuteChanged();
                MoveCustomFieldDownCommand.NotifyCanExecuteChanged();
            });
        }


        partial void OnAppConfigurationServiceChanged(AppConfigurationService value)
        {
            if (value != null)
            {
                value.AppSettings.CustomFields.CollectionChanged += CustomFields_CollectionChanged;

                // Atualiza o estado dos comandos após o serviço ser inicializado
                UpdateMoveCommandsState();

            }
        }

        [RelayCommand]
        private async Task AddCustomField()
        {
            if (Application.Current?.MainPage == null)
                return;

            var result = await Application.Current.Windows[0]?.Page.DisplayPromptAsync(
                ResourceManager["NovoTitulo"],
                ResourceManager["InserirTituloCampo"],
                ResourceManager["Confirmar"],
                ResourceManager["Cancelar"]);

            if (!string.IsNullOrWhiteSpace(result))
            {
                var field = new CustomField
                {
                    Title = result
                };
                AppConfigurationService.AddCustomField(field);
                SelectedCustomField = field;
                OnPropertyChanged(nameof(DefaultFields));
                OnPropertyChanged(nameof(CustomFieldsList));
            }
        }

        [RelayCommand]
        private async Task RemoveCustomField(object? field)
        {
            if (field is not CustomField customField) return;

            if (customField.IsDefault) // Bloqueia remoção de campos padrão
            {
                await Application.Current?.MainPage?.DisplayAlert(
                    ResourceManager["Atencao"],
                    ResourceManager["CampoPadraoNaoPodeExcluir"],
                    ResourceManager["Ok"]);
                return;
            }

            if (Application.Current?.MainPage == null) return;

            var title = ResourceManager["Atencao"];
            var message = string.Format(ResourceManager["ConfirmacaoExcluirCampo"], customField.Title);
            var accept = ResourceManager["Confirmar"];
            var cancel = ResourceManager["Cancelar"];

            bool confirmed = await Application.Current.Windows[0]?.Page.DisplayAlert(title, message, accept, cancel);

            if (confirmed)
            {
                AppConfigurationService.RemoveCustomField(customField);
                if (SelectedCustomField == customField)
                {
                    SelectedCustomField = null;
                }
                OnPropertyChanged(nameof(DefaultFields));
                OnPropertyChanged(nameof(CustomFieldsList));
            }
        }

        [RelayCommand]
        private async Task EditCustomField(object? field)
        {
            if (field is not CustomField customField) return;

            var result = await Application.Current?.Windows[0]?.Page.DisplayPromptAsync(
                ResourceManager["EditarTitulo"],
                ResourceManager["EditarTituloCampo"],
                ResourceManager["Confirmar"],
                ResourceManager["Cancelar"],
                initialValue: customField.Title);

            if (!string.IsNullOrWhiteSpace(result))
            {
                customField.Title = result;
                AppConfigurationService.Save();
                OnPropertyChanged(nameof(DefaultFields));
                OnPropertyChanged(nameof(CustomFieldsList));
            }
        }

        [RelayCommand(CanExecute = nameof(CanMoveCustomFieldUp))]
        private void MoveCustomFieldUp(object? field)
        {
            if (field is not CustomField customField) return;
            var list = AppConfigurationService.AppSettings.CustomFields;
            var index = list.IndexOf(customField);
            if (index > 0 && !customField.IsDefault && !list[index - 1].IsDefault)
            {
                list.Move(index, index - 1);
                AppConfigurationService.Save();
                OnPropertyChanged(nameof(DefaultFields));
                OnPropertyChanged(nameof(CustomFieldsList));
            }
        }

        private bool CanMoveCustomFieldUp(object? field)
        {
            if (field is not CustomField customField) return false;
            var filteredList = CustomFieldsList.ToList();
            // Use ReferenceEquals para garantir que é a mesma instância
            var index = filteredList.FindIndex(f => ReferenceEquals(f, customField));
            return index > 0;
        }

        private bool CanMoveCustomFieldDown(object? field)
        {
            if (field is not CustomField customField) return false;
            var filteredList = CustomFieldsList.ToList();
            // Tenta por referência, se não achar, tenta por valor
            var index = filteredList.FindIndex(f => ReferenceEquals(f, customField)
                || (f.Title == customField.Title && f.IsDefault == customField.IsDefault));
            return index >= 0 && index < filteredList.Count - 1;
        }

        [RelayCommand(CanExecute = nameof(CanMoveCustomFieldDown))]
        private void MoveCustomFieldDown(object? field)
        {
            if (field is not CustomField customField) return;
            var list = AppConfigurationService.AppSettings.CustomFields;
            var index = list.IndexOf(customField);
            if (index >= 0 && index < list.Count - 1 && !customField.IsDefault && !list[index + 1].IsDefault)
            {
                list.Move(index, index + 1);
                AppConfigurationService.Save();
                OnPropertyChanged(nameof(DefaultFields));
                OnPropertyChanged(nameof(CustomFieldsList));
            }
        }

        [RelayCommand]
        private async Task PickLogo()
        {
            if (Application.Current?.MainPage == null)
                return;

            try
            {
                var options = new PickOptions
                {
                    PickerTitle = ResourceManager["SelecionarLogo"],
                    FileTypes = FilePickerFileType.Images
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result == null) return;

                using var stream = await result.OpenReadAsync();
                using var fs = File.Open(_logoFilePath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fs);

                OnPropertyChanged(nameof(LogoImageSource));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    ResourceManager["Erro"],
                    ResourceManager["ErroSelecionarImagem"],
                    ResourceManager["Ok"]);
                Debug.WriteLine($"[PickLogoCommand] Erro: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                AppConfigurationService.Save();
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        ResourceManager["Salvar"],
                        ResourceManager["DadosSalvosComSucesso"],
                        ResourceManager["Ok"]);
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        ResourceManager["Erro"],
                        ResourceManager["ErroAoSalvar"],
                        ResourceManager["Ok"]);
                }
                Debug.WriteLine($"[SaveCommand] Erro: {ex.Message}");
            }
        }

        private void CopyDefaultLogo()
        {
            try
            {
                var task = FileSystem.OpenAppPackageFileAsync(DefaultLogoPath);
                task.Wait();
                using var stream = task.Result;
                using var fs = File.Create(_logoFilePath);
                stream.CopyTo(fs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CopyDefaultLogo] Erro ao copiar logo padrão: {ex.Message}");
            }
        }

        protected override void UpdateLocalizedProperties()
        {

        }
    }
}