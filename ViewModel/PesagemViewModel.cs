using System.Collections.ObjectModel;

using CelmiBluetooth;
using CelmiBluetooth.Devices;
using CelmiBluetooth.Models;
using CelmiBluetooth.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;

namespace AppCelmiMaquinas.ViewModel
{
    /// <summary>
    /// ViewModel para a tela de pesagem que mostra dados do dispositivo conectado.
    /// </summary>
    public partial class PesagemViewModel : ViewModelBase
    {
        private readonly WeightDeviceManager _deviceManager;
        private IDispositivoPesagem? _currentDevice;
        
        #region Properties
        
        /// <summary>
        /// Status do dispositivo conectado.
        /// </summary>
        [ObservableProperty]
        private string _deviceStatus = "Não Conectado";
        
        /// <summary>
        /// Nome do dispositivo conectado.
        /// </summary>
        [ObservableProperty]
        private string _deviceName = "--";
        
        /// <summary>
        /// Porcentagem da bateria.
        /// </summary>
        [ObservableProperty]
        private int _batteryPercentage = 0;
        
        /// <summary>
        /// Peso total formatado.
        /// </summary>
        [ObservableProperty]
        private string _formattedTotalWeight = "0.00 kg";
        
        /// <summary>
        /// Indica se o peso está estável.
        /// </summary>
        [ObservableProperty]
        private bool _isStable = false;
        
        /// <summary>
        /// Status da estabilidade do peso.
        /// </summary>
        [ObservableProperty]
        private string _stabilityStatus = "Instável";
        
        /// <summary>
        /// Peso bruto.
        /// </summary>
        [ObservableProperty]
        private string _grossWeight = "0.00";
        
        /// <summary>
        /// Peso da tara.
        /// </summary>
        [ObservableProperty]
        private string _tareWeight = "0.00";
        
        /// <summary>
        /// Peso líquido.
        /// </summary>
        [ObservableProperty]
        private string _netWeight = "0.00";
        
        /// <summary>
        /// Total de plataformas.
        /// </summary>
        [ObservableProperty]
        private int _totalPlatforms = 0;
        
        /// <summary>
        /// Plataformas conectadas.
        /// </summary>
        [ObservableProperty]
        private int _connectedPlatforms = 0;
        
        /// <summary>
        /// Número da rede.
        /// </summary>
        [ObservableProperty]
        private int _networkId = 0;
        
        /// <summary>
        /// Lista de pesos das plataformas.
        /// </summary>
        public ObservableCollection<PlataformaDados> PlatformWeights { get; } = new();
        
        /// <summary>
        /// Indica se há um dispositivo conectado.
        /// </summary>
        public bool IsConnected => _currentDevice?.Conectado ?? false;
        
        #endregion
        
        /// <summary>
        /// Construtor do PesagemViewModel.
        /// </summary>
        public PesagemViewModel(ILocalizationResourceManager resourceManager, WeightDeviceManager deviceManager) 
            : base(resourceManager)
        {
            _deviceManager = deviceManager;
            
            // Inicializa o ViewModel e começa a monitorar o dispositivo
            InitializeAsync();
        }
        
        /// <summary>
        /// Inicializa o ViewModel e começa a monitorar o dispositivo atual.
        /// </summary>
        private async void InitializeAsync()
        {
            await _deviceManager.InitializeAsync();
            
            // Iniciar monitoramento do dispositivo
            StartDeviceMonitoring();
            
            // Timer para atualizar os dados periodicamente
            var timer = new System.Timers.Timer(500); // Atualiza a cada 500ms
            timer.Elapsed += (s, e) => UpdateDeviceData();
            timer.Start();
        }
        
        /// <summary>
        /// Monitora alterações no dispositivo atual.
        /// </summary>
        private void StartDeviceMonitoring()
        {
            _deviceManager.PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(WeightDeviceManager.CurrentDevice) || 
                    e.PropertyName == nameof(WeightDeviceManager.IsDeviceConnected))
                {
                    _currentDevice = _deviceManager.CurrentDevice;
                    UpdateDeviceData();
                    OnPropertyChanged(nameof(IsConnected));
                }
            };
            
            // Verifica se já existe um dispositivo conectado
            _currentDevice = _deviceManager.CurrentDevice;
            UpdateDeviceData();
            OnPropertyChanged(nameof(IsConnected));
        }
        
        /// <summary>
        /// Atualiza os dados do dispositivo na UI.
        /// </summary>
        private void UpdateDeviceData()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_currentDevice == null || !_currentDevice.Conectado)
                {
                    DeviceStatus = "Não Conectado";
                    DeviceName = "--";
                    BatteryPercentage = 0;
                    FormattedTotalWeight = "0.00 kg";
                    IsStable = false;
                    StabilityStatus = "Instável";
                    GrossWeight = "0.00";
                    TareWeight = "0.00";
                    NetWeight = "0.00";
                    TotalPlatforms = 0;
                    ConnectedPlatforms = 0;
                    NetworkId = 0;
                    PlatformWeights.Clear();
                    return;
                }
                
                // Atualizar status do dispositivo
                DeviceStatus = "Conectado";
                DeviceName = _currentDevice.Nome;
                FormattedTotalWeight = _currentDevice.ObterPesoTotalFormatado();
                
                if (_currentDevice is VirtualRXDevice virtualDevice)
                {
                    // Atualizar dados específicos do dispositivo virtual RX
                    var geral = virtualDevice.Geral;
                    if (geral != null)
                    {
                        BatteryPercentage = geral.PorcentagemBateria;
                        IsStable = !virtualDevice.AlgumaPlataformaNaoEstavel;
                        StabilityStatus = IsStable ? "Estável" : "Instável";
                        
                        var casasDecimais = geral.CasasDecimais;
                        var format = $"F{casasDecimais}";
                        
                        GrossWeight = geral.PesoBruto.ToString(format);
                        TareWeight = (geral.PesoBruto - geral.Peso).ToString(format);
                        NetWeight = geral.Peso.ToString(format);
                        
                        TotalPlatforms = geral.QuantidadePlataformas;
                        ConnectedPlatforms = virtualDevice.Plataformas.ToArray().Count(p => p != null && p.Conectada);
                        NetworkId = 1; // RX virtual sempre usa rede 1
                        
                        // Atualizar informações das plataformas
                        UpdatePlatformWeights(virtualDevice);
                    }
                }
                else if (_currentDevice is PhysicalRXDevice physicalDevice)
                {
                    // Atualizar dados específicos do dispositivo físico RX
                    var geral = physicalDevice.Geral;
                    if (geral != null)
                    {
                        BatteryPercentage = geral.PorcentagemBateria;
                        IsStable = !physicalDevice.AlgumaPlataformaNaoEstavel;
                        StabilityStatus = IsStable ? "Estável" : "Instável";
                        
                        var casasDecimais = geral.CasasDecimais;
                        var format = $"F{casasDecimais}";
                        
                        GrossWeight = geral.PesoBruto.ToString(format);
                        TareWeight = (geral.PesoBruto - geral.Peso).ToString(format);
                        NetWeight = geral.Peso.ToString(format);
                        
                        TotalPlatforms = geral.QuantidadePlataformas;
                        ConnectedPlatforms = physicalDevice.Plataformas?.Count(p => p != null && p.Conectada) ?? 0;
                        NetworkId = geral.QuantidadePlataformas;
                        
                        // Atualizar informações das plataformas
                        UpdatePlatformWeights(physicalDevice);
                    }
                }
            });
        }
        
        /// <summary>
        /// Atualiza as informações das plataformas individuais.
        /// </summary>
        private void UpdatePlatformWeights(VirtualRXDevice device)
        {
            var platforms = device.Plataformas.ToArray();
            if (platforms == null) return;
            
            // Atualizar ou adicionar plataformas
            for (int i = 0; i < platforms.Length; i++)
            {
                var platform = platforms[i];
                if (platform == null) continue;
                
                var platformId = i + 1;
                var battery = platform.PorcentagemBateria;
                var existingPlatform = PlatformWeights.FirstOrDefault(p => p.PlatformId == platformId);
                
                if (existingPlatform != null)
                {
                    // Atualizar plataforma existente
                    existingPlatform.Update(platformId, platform.Nome, device.GetFormattedWeight(i),
                        platform.PesagemEstavel, platform.Peso, platform.PesoBruto, platform.Conectada, battery);
                }
                else
                {
                    // Adicionar nova plataforma
                    PlatformWeights.Add(new PlataformaDados(
                        platformId, platform.Nome, device.GetFormattedWeight(i),
                        platform.PesagemEstavel, platform.Peso, platform.PesoBruto, platform.Conectada, battery));
                }
            }
            
            // Remover plataformas que não existem mais
            for (int i = PlatformWeights.Count - 1; i >= 0; i--)
            {
                var platformId = PlatformWeights[i].PlatformId;
                if (platformId > platforms.Length || platforms[platformId - 1] == null)
                {
                    PlatformWeights.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Atualiza as informações das plataformas individuais.
        /// </summary>
        private void UpdatePlatformWeights(PhysicalRXDevice device)
        {
            var platforms = device.Plataformas;
            if (platforms == null) return;
            
            // Atualizar ou adicionar plataformas
            for (int i = 0; i < platforms.Length; i++)
            {
                var platform = platforms[i];
                if (platform == null) continue;
                
                var platformId = i + 1;
                var battery = platform.PorcentagemBateria;
                var existingPlatform = PlatformWeights.FirstOrDefault(p => p.PlatformId == platformId);
                
                if (existingPlatform != null)
                {
                    // Atualizar plataforma existente
                    existingPlatform.Update(platformId, platform.Nome, device.GetPesoFormatado(platformId),
                        platform.PesagemEstavel, platform.Peso, platform.PesoBruto, platform.Conectada, battery);
                }
                else
                {
                    // Adicionar nova plataforma
                    PlatformWeights.Add(new PlataformaDados(
                        platformId, platform.Nome, device.GetPesoFormatado(platformId),
                        platform.PesagemEstavel, platform.Peso, platform.PesoBruto, platform.Conectada, battery));
                }
            }
            
            // Remover plataformas que não existem mais
            for (int i = PlatformWeights.Count - 1; i >= 0; i--)
            {
                var platformId = PlatformWeights[i].PlatformId;
                if (platformId > platforms.Length || platforms[platformId - 1] == null)
                {
                    PlatformWeights.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Comando para zerar o peso.
        /// </summary>
        [RelayCommand]
        private async Task ZeroAsync()
        {
            if (_currentDevice is VirtualRXDevice virtualDevice)
            {
                // Implementar lógica para zerar o peso no dispositivo virtual
                await virtualDevice.ZerarTodasPlataformasAsync();
            }
            else if (_currentDevice is PhysicalRXDevice physicalDevice)
            {
                // Zerar o peso no dispositivo físico
                await physicalDevice.ZeroAllPlatformsAsync();
            }
        }
        
        /// <summary>
        /// Comando para aplicar a tara.
        /// </summary>
        [RelayCommand]
        private async Task TareAsync()
        {
            if (_currentDevice is VirtualRXDevice virtualDevice)
            {
                // Implementar lógica para aplicar a tara no dispositivo virtual
                await virtualDevice.TararTodasPlataformasAsync();
            }
            else if (_currentDevice is PhysicalRXDevice physicalDevice)
            {
                // Aplicar a tara no dispositivo físico
                await physicalDevice.TareAllPlatformsAsync();
            }
        }
        
        /// <summary>
        /// Atualiza as propriedades localizadas.
        /// </summary>
        protected override void UpdateLocalizedProperties()
        {
            // Atualizar strings que usam localização
            if (!IsConnected)
            {
                DeviceStatus = ResourceManager["NãoConectado"] ?? "Não Conectado";
            }
            else
            {
                DeviceStatus = ResourceManager["Conectado"] ?? "Conectado";
            }
            
            StabilityStatus = IsStable ? 
                (ResourceManager["Estável"] ?? "Estável") : 
                (ResourceManager["Instável"] ?? "Instável");
        }
    }
}