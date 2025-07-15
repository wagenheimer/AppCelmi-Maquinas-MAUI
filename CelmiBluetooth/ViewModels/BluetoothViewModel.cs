using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using System.Collections.ObjectModel;
using Shiny.BluetoothLE;
using AppCelmiMaquinas.Models;
using Microsoft.Maui.ApplicationModel;
using CelmiBluetooth;
using CelmiBluetooth.Models;
using CelmiBluetooth.Devices;

namespace CelmiBluetooth.ViewModels
{
    /// <summary>
    /// ViewModel para gerenciar conexões Bluetooth com dispositivos RX e plataformas.
    /// </summary>
    public partial class BluetoothViewModel : ViewModelBase, IDisposable
    {
        private readonly IBleManager _bleManager;
        private readonly WeightDeviceManager _deviceManager;
        private bool _disposed = false;

        #region Observable Properties with AOT compatibility

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isConnectedToRX;

        partial void OnIsConnectedToRXChanged(bool value)
        {
            OnPropertyChanged(nameof(IsRXOptionsVisible));
            OnPropertyChanged(nameof(IsPlatformsOptionsVisible));
        }

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isConnectedToPlatforms;

        partial void OnIsConnectedToPlatformsChanged(bool value)
        {
            OnPropertyChanged(nameof(IsRXOptionsVisible));
            OnPropertyChanged(nameof(IsPlatformsOptionsVisible));
        }

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isConnectingToRX;

        partial void OnIsConnectingToRXChanged(bool value)
        {
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsRXOptionsVisible));
            OnPropertyChanged(nameof(IsPlatformsOptionsVisible));
        }

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isConnectingToPlatforms;

        partial void OnIsConnectingToPlatformsChanged(bool value)
        {
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsRXOptionsVisible));
            OnPropertyChanged(nameof(IsPlatformsOptionsVisible));
        }

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isScanningForDevices;

        partial void OnIsScanningForDevicesChanged(bool value)
        {
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsRXOptionsVisible));
            OnPropertyChanged(nameof(IsPlatformsOptionsVisible));
        }

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isDeviceListVisible;

        partial void OnIsDeviceListVisibleChanged(bool value)
        {
            OnPropertyChanged(nameof(IsRXOptionsVisible));
            OnPropertyChanged(nameof(IsPlatformsOptionsVisible));
        }

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _connectionStatus = "Não Conectado";

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private BleDevice? _selectedRXDevice;

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private int _selectedNetworkNumber = 1;

        [ObservableProperty]
        [property: global::CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private int _selectedTotalPlatforms = 2;

        #endregion

        /// <summary>
        /// Lista de dispositivos BLE encontrados durante o scan.
        /// </summary>
        public ObservableCollection<BleDevice> DiscoveredDevices { get; } = new();

        /// <summary>
        /// Lista de plataformas BLE-TX encontradas durante o scan.
        /// </summary>
        public ObservableCollection<BleDevice> DiscoveredTXPlatforms { get; } = new();

        /// <summary>
        /// Lista de totais de plataformas disponíveis.
        /// </summary>
        public ObservableCollection<int> TotalPlatforms { get; } = new();

        /// <summary>
        /// Token de cancelamento para operações de conexão.
        /// </summary>
        private CancellationTokenSource? _connectionCancellationTokenSource;

        /// <summary>
        /// Token de cancelamento para operações de scan.
        /// </summary>
        private CancellationTokenSource? _scanCancellationTokenSource;

        /// <summary>
        /// Dispositivo atualmente conectado.
        /// </summary>
        private IPeripheral? _connectedPeripheral;

        /// <summary>
        /// Subscription para o scan de dispositivos.
        /// </summary>
        private IDisposable? _scanSubscription;

        /// <summary>
        /// Indica se algum dispositivo está conectado (RX ou Plataformas).
        /// </summary>
        public bool IsAnyDeviceConnected => IsConnectedToRX || IsConnectedToPlatforms;

        /// <summary>
        /// Indica se está em processo de conexão.
        /// </summary>
        public bool IsConnecting => IsConnectingToRX || IsConnectingToPlatforms;

        /// <summary>
        /// Indica se está ocupado (conectando ou fazendo scan).
        /// </summary>
        public bool IsBusy => IsConnecting || IsScanningForDevices;

        /// <summary>
        /// Indica se as opções do dispositivo RX devem estar visíveis.
        /// </summary>
        public bool IsRXOptionsVisible => !IsBusy && !IsDeviceListVisible &&
                                         !IsConnectingToPlatforms && !IsConnectedToPlatforms;

        /// <summary>
        /// Indica se as opções de plataformas devem estar visíveis.
        /// </summary>
        public bool IsPlatformsOptionsVisible => !IsBusy && !IsDeviceListVisible &&
                                                !IsConnectingToRX && !IsConnectedToRX;

        /// <summary>
        /// Construtor do BluetoothViewModel.
        /// </summary>
        /// <param name="resourceManager">Gerenciador de recursos de localização.</param>
        /// <param name="bleManager">Gerenciador de Bluetooth LE do Shiny.</param>
        /// <param name="deviceManager">Gerenciador de dispositivos de pesagem.</param>
        public BluetoothViewModel(
            ILocalizationResourceManager resourceManager,
            IBleManager bleManager,
            WeightDeviceManager deviceManager) : base(resourceManager)
        {
            _bleManager = bleManager;
            _deviceManager = deviceManager;
            InitializeCollections();

            // Monitorar alterações no dispositivo atual do WeightDeviceManager
            _deviceManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(WeightDeviceManager.CurrentDevice) ||
                    e.PropertyName == nameof(WeightDeviceManager.IsDeviceConnected))
                {
                    UpdateConnectionStatusFromManager();
                }
            };

            // Verificar se já existe um dispositivo conectado
            UpdateConnectionStatusFromManager();
        }

        /// <summary>
        /// Atualiza o status de conexão com base no WeightDeviceManager
        /// </summary>
        private void UpdateConnectionStatusFromManager()
        {
            var currentDevice = _deviceManager.CurrentDevice;

            if (currentDevice != null && currentDevice.Conectado)
            {
                if (currentDevice is PhysicalRXDevice physicalRX)
                {
                    // Atualizar UI para mostrar que estamos conectados a um dispositivo RX físico
                    IsConnectedToRX = true;
                    IsConnectedToPlatforms = false;

                    var device = DiscoveredDevices.FirstOrDefault(d => d.Address == physicalRX.DeviceId);
                    if (device != null)
                    {
                        device.IsConnected = true;
                        SelectedRXDevice = device;
                    }
                    else
                    {
                        // Criar um dispositivo BLE temporário para representar o dispositivo conectado
                        SelectedRXDevice = new BleDevice
                        {
                            Name = physicalRX.DeviceName,
                            Address = physicalRX.DeviceId,
                            IsConnected = true
                        };
                    }

                    ConnectionStatus = $"Conectado ao {physicalRX.DeviceName}";
                }
                else if (currentDevice is VirtualRXDevice)
                {
                    // Atualizar UI para mostrar que estamos conectados a um dispositivo RX virtual
                    IsConnectedToRX = true;
                    IsConnectedToPlatforms = false;
                    ConnectionStatus = "Conectado ao dispositivo virtual";
                }
            }
            else
            {
                // Não há dispositivo conectado
                if (_connectedPeripheral == null)
                {
                    IsConnectedToRX = false;
                    IsConnectedToPlatforms = false;
                    ConnectionStatus = "Não Conectado";
                }
            }

            OnPropertyChanged(nameof(IsAnyDeviceConnected));
        }

        /// <summary>
        /// Inicializa as coleções de total de plataformas.
        /// </summary>
        private void InitializeCollections()
        {
            // Total de plataformas de 1 a 8
            for (int i = 1; i <= 8; i++)
            {
                TotalPlatforms.Add(i);
            }
        }

        /// <summary>
        /// Comando para conectar ao dispositivo RX (Visor).
        /// </summary>
        [RelayCommand]
        private async Task ConnectToRXDeviceAsync()
        {
            if (_disposed) return;

            try
            {
                // If connected to platforms, disconnect first
                if (IsConnectedToPlatforms)
                {
                    await DisconnectAsync();
                }

                // Verificar permissões de Bluetooth
                var bluetoothPermission = await Permissions.RequestAsync<Permissions.Bluetooth>();
                if (bluetoothPermission != PermissionStatus.Granted)
                {
                    ConnectionStatus = "Permissão de Bluetooth negada";
                    return;
                }

                // Iniciar scan por dispositivos BLE-RX (não vamos verificar se está disponível aqui)
                await StartScanForRXDevicesAsync();
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Erro: {ex.Message}";
            }
        }

        /// <summary>
        /// Inicia o scan por dispositivos BLE com "BLE-RX" no nome.
        /// </summary>
        private async Task StartScanForRXDevicesAsync()
        {
            try
            {
                IsScanningForDevices = true;
                IsDeviceListVisible = true;
                ConnectionStatus = "Procurando dispositivos BLE-RX...";

                // Limpar lista anterior
                DiscoveredDevices.Clear();

                _scanCancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _scanCancellationTokenSource.Token;

                // Iniciar scan sem filtros específicos (vamos filtrar os resultados)
                _scanSubscription = _bleManager
                    .Scan()
                    .Subscribe(
                        scanResult => MainThread.BeginInvokeOnMainThread(() => ProcessScanResult(scanResult)),
                        error => MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                ConnectionStatus = $"Erro no scan: {error.Message}";
                            }
                        })
                    );

                // Aguardar 10 segundos para encontrar dispositivos, mas verificar cancelamento a cada segundo
                for (int i = 0; i < 10; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    await Task.Delay(1000, cancellationToken);
                }

                // Parar o scan
                await StopScanAsync();

                if (!cancellationToken.IsCancellationRequested)
                {
                    if (DiscoveredDevices.Count == 0)
                    {
                        ConnectionStatus = "Nenhum dispositivo BLE-RX encontrado";
                    }
                    else
                    {
                        ConnectionStatus = $"{DiscoveredDevices.Count} dispositivo(s) BLE-RX encontrado(s)";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ConnectionStatus = "Scan cancelado";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Erro no scan: {ex.Message}";
            }
            finally
            {
                IsScanningForDevices = false;
                await StopScanAsync();
                _scanCancellationTokenSource?.Dispose();
                _scanCancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Processa o resultado do scan e adiciona dispositivos BLE-RX.
        /// </summary>
        /// <param name="scanResult">Resultado do scan.</param>
        private void ProcessScanResult(ScanResult scanResult)
        {
            // Filtrar apenas dispositivos com "BLE-RX" no nome
            if (string.IsNullOrEmpty(scanResult.Peripheral.Name) ||
                !scanResult.Peripheral.Name.Contains("BLE-RX", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var existing = DiscoveredDevices.FirstOrDefault(d => d.Address == scanResult.Peripheral.Uuid);
            if (existing != null)
            {
                // Atualizar RSSI se dispositivo já existe
                existing.Rssi = scanResult.Rssi;
                return;
            }

            var device = new BleDevice
            {
                Name = scanResult.Peripheral.Name ?? "Dispositivo Desconhecido",
                Address = scanResult.Peripheral.Uuid,
                Rssi = scanResult.Rssi,
                Peripheral = scanResult.Peripheral
            };

            DiscoveredDevices.Add(device);
        }

        /// <summary>
        /// Para o scan de dispositivos BLE.
        /// </summary>
        private async Task StopScanAsync()
        {
            _scanSubscription?.Dispose();
            _scanSubscription = null;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Comando para conectar às plataformas.
        /// </summary>
        [RelayCommand]
        private async Task ConnectToPlatformsAsync()
        {
            if (_disposed) return;

            try
            {
                // If connected to RX, disconnect first
                if (IsConnectedToRX)
                {
                    await DisconnectFromRXAsync();
                    IsConnectedToRX = false;
                }

                IsConnectingToPlatforms = true;
                ConnectionStatus = "Procurando plataformas BLE-TX...";

                // Limpar lista anterior
                DiscoveredTXPlatforms.Clear();

                _scanCancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _scanCancellationTokenSource.Token;

                // Iniciar scan sem filtros específicos (vamos filtrar os resultados)
                _scanSubscription = _bleManager
                    .Scan()
                    .Subscribe(
                        scanResult => MainThread.BeginInvokeOnMainThread(() => ProcessTXScanResult(scanResult)),
                        error => MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                ConnectionStatus = $"Erro no scan: {error.Message}";
                            }
                        })
                    );

                // Aguardar 10 segundos para encontrar dispositivos, mas verificar cancelamento a cada segundo
                for (int i = 0; i < 10; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    await Task.Delay(1000, cancellationToken);
                }

                // Parar o scan
                await StopScanAsync();

                if (!cancellationToken.IsCancellationRequested)
                {
                    if (DiscoveredTXPlatforms.Count == 0)
                    {
                        ConnectionStatus = "Nenhuma plataforma BLE-TX encontrada";
                    }
                    else
                    {
                        ConnectionStatus = $"{DiscoveredTXPlatforms.Count} plataforma(s) BLE-TX encontrada(s)";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ConnectionStatus = "Scan cancelado";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Erro no scan: {ex.Message}";
            }
            finally
            {
                IsConnectingToPlatforms = false;
                await StopScanAsync();
                _scanCancellationTokenSource?.Dispose();
                _scanCancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Processa o resultado do scan e adiciona plataformas BLE-TX.
        /// </summary>
        /// <param name="scanResult">Resultado do scan.</param>
        private void ProcessTXScanResult(ScanResult scanResult)
        {
            // Filtrar apenas dispositivos com "BLE-TX" no nome
            if (string.IsNullOrEmpty(scanResult.Peripheral.Name) ||
                !scanResult.Peripheral.Name.Contains("BLE-TX", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var existing = DiscoveredTXPlatforms.FirstOrDefault(d => d.Address == scanResult.Peripheral.Uuid);
            if (existing != null)
            {
                // Atualizar RSSI se dispositivo já existe
                existing.Rssi = scanResult.Rssi;
                return;
            }

            var device = new BleDevice
            {
                Name = scanResult.Peripheral.Name ?? "Plataforma Desconhecida",
                Address = scanResult.Peripheral.Uuid,
                Rssi = scanResult.Rssi,
                Peripheral = scanResult.Peripheral
            };

            DiscoveredTXPlatforms.Add(device);
        }

        /// <summary>
        /// Comando para cancelar a conexão em andamento.
        /// </summary>
        [RelayCommand]
        private async Task CancelConnectionAsync()
        {
            try
            {
                // Cancelar tokens de cancelamento
                _connectionCancellationTokenSource?.Cancel();
                _scanCancellationTokenSource?.Cancel();

                // Se estiver fazendo scan, parar e esconder lista
                if (IsScanningForDevices)
                {
                    await StopScanAsync();
                    IsDeviceListVisible = false;
                    IsScanningForDevices = false;
                }

                // Limpar estados de conexão
                if (IsConnectingToRX)
                {
                    IsConnectingToRX = false;
                }

                if (IsConnectingToPlatforms)
                {
                    IsConnectingToPlatforms = false;
                }

                // Atualizar status
                ConnectionStatus = "Operação cancelada";

                // Limpar tokens
                _connectionCancellationTokenSource?.Dispose();
                _connectionCancellationTokenSource = null;
                _scanCancellationTokenSource?.Dispose();
                _scanCancellationTokenSource = null;
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Erro ao cancelar: {ex.Message}";
            }
        }

        /// <summary>
        /// Comando para ocultar a lista de dispositivos.
        /// </summary>
        [RelayCommand]
        private async Task HideDeviceListAsync()
        {
            IsDeviceListVisible = false;
            if (IsScanningForDevices)
            {
                _scanCancellationTokenSource?.Cancel();
                await StopScanAsync();
            }
        }

        /// <summary>
        /// Desconecta do dispositivo RX atual.
        /// </summary>
        private async Task DisconnectFromRXAsync()
        {
            if (_connectedPeripheral != null)
            {
                try
                {
                    // Verificar se o dispositivo já está sendo gerenciado pelo WeightDeviceManager
                    if (_deviceManager.CurrentDevice != null &&
                        _deviceManager.CurrentDevice is PhysicalRXDevice physicalDevice &&
                        physicalDevice.DeviceId == _connectedPeripheral.Uuid)
                    {
                        // Desconectar através do WeightDeviceManager
                        await _deviceManager.DisconnectCurrentDeviceAsync();
                    }
                    else
                    {
                        // Desconectar diretamente
                        _connectedPeripheral.CancelConnection();
                    }

                    if (SelectedRXDevice != null)
                    {
                        SelectedRXDevice.IsConnected = false;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao desconectar RX: {ex.Message}");
                }
                finally
                {
                    _connectedPeripheral = null;
                    SelectedRXDevice = null;
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Comando para desconectar de todos os dispositivos.
        /// </summary>
        [RelayCommand]
        private async Task DisconnectAsync()
        {
            try
            {
                ConnectionStatus = "Desconectando...";

                // Sempre usar o WeightDeviceManager para desconectar
                await _deviceManager.DisconnectCurrentDeviceAsync();

                if (IsConnectedToRX)
                {
                    await DisconnectFromRXAsync();
                    IsConnectedToRX = false;
                }

                IsConnectedToPlatforms = false;
                ConnectionStatus = "Desconectado";
                OnPropertyChanged(nameof(IsAnyDeviceConnected));
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Erro: {ex.Message}";
            }
        }

        /// <summary>
        /// Atualiza as propriedades localizadas quando a cultura muda.
        /// </summary>
        protected override void UpdateLocalizedProperties()
        {
            // Atualizar status baseado no estado atual se necessário
            if (!IsConnectedToRX && !IsConnectedToPlatforms && !IsConnecting)
            {
                ConnectionStatus = ResourceManager["NãoConectado"] ?? "Não Conectado";
            }
            else
            {
                DeviceStatus = ResourceManager["Conectado"] ?? "Conectado";
            }

            StabilityStatus = IsStable ?
                (ResourceManager["Estável"] ?? "Estável") :
                (ResourceManager["Instável"] ?? "Instável");
        }

        [RelayCommand]
        private void IncrementNetwork()
        {
            if (SelectedNetworkNumber < 255)
                SelectedNetworkNumber++;
        }

        [RelayCommand]
        private void DecrementNetwork()
        {
            if (SelectedNetworkNumber > 1)
                SelectedNetworkNumber--;
        }

        [RelayCommand]
        private void IncrementPlatforms()
        {
            if (SelectedTotalPlatforms < 8)
                SelectedTotalPlatforms++;
        }

        [RelayCommand]
        private void DecrementPlatforms()
        {
            if (SelectedTotalPlatforms > 1)
                SelectedTotalPlatforms--;
        }

        /// <summary>
        /// Libera todos os recursos utilizados pelo BluetoothViewModel.
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera os recursos gerenciados e não-gerenciados utilizados pelo BluetoothViewModel.
        /// </summary>
        /// <param name="disposing">true se chamado de Dispose(); false se chamado do finalizador.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    // Cancelar todas as operações em andamento
                    _connectionCancellationTokenSource?.Cancel();
                    _scanCancellationTokenSource?.Cancel();

                    // Parar scan e desconectar dispositivos
                    if (_scanSubscription != null)
                    {
                        _scanSubscription.Dispose();
                        _scanSubscription = null;
                    }

                    // Dispositivos agora são gerenciados pelo WeightDeviceManager
                    // Não precisamos desconectá-los diretamente aqui

                    // Liberar CancellationTokenSources
                    _connectionCancellationTokenSource?.Dispose();
                    _connectionCancellationTokenSource = null;

                    _scanCancellationTokenSource?.Dispose();
                    _scanCancellationTokenSource = null;

                    // Limpar coleções
                    DiscoveredDevices.Clear();
                    DiscoveredTXPlatforms.Clear();

                    // Reset estados
                    IsScanningForDevices = false;
                    IsConnectingToRX = false;
                    IsConnectingToPlatforms = false;
                    IsConnectedToRX = false;
                    IsConnectedToPlatforms = false;
                    IsDeviceListVisible = false;
                    SelectedRXDevice = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro durante dispose do BluetoothViewModel: {ex.Message}");
                }

                _disposed = true;
            }

            // Chamar o dispose da classe base
            base.Dispose(disposing);
        }

        /// <summary>
        /// Propriedade para compatibilidade com o código existente
        /// </summary>
        private string DeviceStatus { get; set; } = string.Empty;

        /// <summary>
        /// Propriedade para compatibilidade com o código existente
        /// </summary>
        private bool IsStable { get; set; }

        /// <summary>
        /// Propriedade para compatibilidade com o código existente
        /// </summary>
        private string StabilityStatus { get; set; } = string.Empty;

        /// <summary>
        /// Finalizador para garantir que os recursos sejam liberados mesmo se Dispose não for chamado.
        /// </summary>
        ~BluetoothViewModel()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Classe auxiliar para carregar as informações do dispositivo Celmi
    /// </summary>
    public class CelmiDeviceInfo
    {
        /// <summary>
        /// Nome do dispositivo 
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço do dispositivo
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Número de plataformas
        /// </summary>
        public int Plataformas { get; set; } = 1;
    }
}