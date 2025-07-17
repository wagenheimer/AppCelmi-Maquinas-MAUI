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
using System.ComponentModel;

namespace CelmiBluetooth.ViewModels
{
    /// <summary>
    /// Enumera os modos de conexão disponíveis.
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// Modo de conexão com dispositivo RX (Visor).
        /// </summary>
        RX,
        /// <summary>
        /// Modo de conexão com Plataformas TX.
        /// </summary>
        TX
    }

    /// <summary>
    /// ViewModel para gerenciar conexões Bluetooth com dispositivos RX e plataformas.
    /// </summary>
    public partial class BluetoothViewModel : ViewModelBase, IDisposable
    {
        private readonly IBleManager _bleManager;
        private readonly WeightDeviceManager _deviceManager;
        private bool _disposed = false;

        #region Observable Properties

        /// <summary>
        /// Modo de conexão selecionado (RX ou TX).
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRXModeSelected))]
        [NotifyPropertyChangedFor(nameof(IsTXModeSelected))]
        [NotifyPropertyChangedFor(nameof(IsRXOptionsVisible))]
        [NotifyPropertyChangedFor(nameof(IsPlatformsOptionsVisible))]
        private ConnectionMode selectedMode = ConnectionMode.RX;

        /// <summary>
        /// Indica se está conectado ao dispositivo RX.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsConnectedOrConnectingToRX))]
        [NotifyPropertyChangedFor(nameof(IsAnyDeviceConnected))]
        [NotifyPropertyChangedFor(nameof(IsRXOptionsVisible))]
        [NotifyPropertyChangedFor(nameof(IsPlatformsOptionsVisible))]
        [NotifyPropertyChangedFor(nameof(CanSelectMode))]
        [NotifyPropertyChangedFor(nameof(CanShowSearchButton))]
        private bool isConnectedToRX;

        partial void OnIsConnectedToRXChanged(bool value)
        {
            System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] IsConnectedToRXChanged {value}");
        }

        /// <summary>
        /// Indica se está conectado às plataformas.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyDeviceConnected))]
        [NotifyPropertyChangedFor(nameof(IsRXOptionsVisible))]
        [NotifyPropertyChangedFor(nameof(IsPlatformsOptionsVisible))]
        [NotifyPropertyChangedFor(nameof(CanSelectMode))]
        [NotifyPropertyChangedFor(nameof(CanShowSearchButton))]
        private bool isConnectedToPlatforms;

        /// <summary>
        /// Indica se está conectando ao dispositivo RX.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsConnectedOrConnectingToRX))]
        [NotifyPropertyChangedFor(nameof(IsConnecting))]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(CanSwitchMode))]
        [NotifyPropertyChangedFor(nameof(CanSelectMode))]
        [NotifyPropertyChangedFor(nameof(CanShowSearchButton))]
        private bool isConnectingToRX;

        /// <summary>
        /// Indica se está conectando às plataformas.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsConnecting))]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(CanSwitchMode))]
        [NotifyPropertyChangedFor(nameof(CanSelectMode))]
        [NotifyPropertyChangedFor(nameof(CanShowSearchButton))]
        private bool isConnectingToPlatforms;

        /// <summary>
        /// Indica se está fazendo scan de dispositivos.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(CanSwitchMode))]
        private bool isScanningForDevices;

        /// <summary>
        /// Indica se a lista de dispositivos está visível.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRXOptionsVisible))]
        [NotifyPropertyChangedFor(nameof(IsPlatformsOptionsVisible))]
        private bool isDeviceListVisible;

        /// <summary>
        /// Status da conexão atual.
        /// </summary>
        [ObservableProperty]
        private string connectionStatus = "Não Conectado";

        /// <summary>
        /// Dispositivo RX selecionado.
        /// </summary>
        [ObservableProperty]
        private BleDevice? selectedRXDevice;

        /// <summary>
        /// Número da rede selecionada.
        /// </summary>
        [ObservableProperty]
        private int selectedNetworkNumber = 1;

        /// <summary>
        /// Total de plataformas selecionadas.
        /// </summary>
        [ObservableProperty]
        private int selectedTotalPlatforms = 2;

        /// <summary>
        /// Status do dispositivo.
        /// </summary>
        [ObservableProperty]
        private string deviceStatus = string.Empty;

        /// <summary>
        /// Indica se o peso está estável.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StabilityStatus))]
        private bool isStable;

        /// <summary>
        /// Status da estabilidade.
        /// </summary>
        [ObservableProperty]
        private string stabilityStatus = string.Empty;

        #endregion

        #region Event Handlers

        partial void OnSelectedModeChanged(ConnectionMode value)
        {
            // Cancelar operações em andamento quando trocar de modo
            if (IsBusy)
            {
                _ = CancelConnectionAsync();
            }
        }

        partial void OnIsConnectingToRXChanged(bool value)
        {
            SwitchToRXModeCommand.NotifyCanExecuteChanged();
            SwitchToTXModeCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsConnectingToPlatformsChanged(bool value)
        {
            SwitchToRXModeCommand.NotifyCanExecuteChanged();
            SwitchToTXModeCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsScanningForDevicesChanged(bool value)
        {
            SwitchToRXModeCommand.NotifyCanExecuteChanged();
            SwitchToTXModeCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsStableChanged(bool value)
        {
            StabilityStatus = value ?
                (ResourceManager["Estável"] ?? "Estável") :
                (ResourceManager["Instável"] ?? "Instável");
        }

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
        /// Indica se o modo RX está selecionado.
        /// </summary>
        public bool IsRXModeSelected => SelectedMode == ConnectionMode.RX;

        /// <summary>
        /// Indica se o modo TX está selecionado.
        /// </summary>
        public bool IsTXModeSelected => SelectedMode == ConnectionMode.TX;

        /// <summary>
        /// Indica se as opções do dispositivo RX devem estar visíveis.
        /// </summary>
        public bool IsRXOptionsVisible => IsRXModeSelected;

        /// <summary>
        /// Indica se as opções de plataformas devem estar visíveis.
        /// </summary>
        public bool IsPlatformsOptionsVisible => IsTXModeSelected;

        /// <summary>
        /// Indica se é possível trocar de modo (não durante operações).
        /// </summary>
        public bool CanSwitchMode => !IsBusy;

        /// <summary>
        /// Indica se o seletor de modo deve estar habilitado.
        /// Desabilitado quando há dispositivos conectados ou conectando.
        /// </summary>
        public bool CanSelectMode => !IsAnyDeviceConnected && !IsConnecting;

        /// <summary>
        /// Indica se o botão "Buscar Dispositivos BLE-RX" deve estar visível.
        /// Oculto quando há dispositivos conectados ou conectando.
        /// </summary>
        public bool CanShowSearchButton => !IsAnyDeviceConnected && !IsConnecting;

        /// <summary>
        /// Indica se está conectado ou conectando ao RX.
        /// </summary>
        public bool IsConnectedOrConnectingToRX => IsConnectedToRX || IsConnectingToRX;

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
                    e.PropertyName == nameof(WeightDeviceManager.IsDeviceConnected) ||
                    e.PropertyName == nameof(WeightDeviceManager.ConnectionPhase))
                {
                    System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] PropertyChanged detectado: {e.PropertyName}");

                    // Se CurrentDevice mudou, reconfigurar monitoramento
                    if (e.PropertyName == nameof(WeightDeviceManager.CurrentDevice))
                    {
                        SetupCurrentDeviceMonitoring();
                    }

                    UpdateConnectionStatusFromManager();
                }
            };

            // Verificar se já existe um dispositivo conectado
            UpdateConnectionStatusFromManager();
            SetupCurrentDeviceMonitoring();
        }

        /// <summary>
        /// Atualiza o status de conexão com base no WeightDeviceManager
        /// </summary>
        private void UpdateConnectionStatusFromManager()
        {
            // Garantir que as atualizações da UI sejam feitas no thread principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var currentDevice = _deviceManager.CurrentDevice;
                var isDeviceConnected = _deviceManager.IsDeviceConnected;

                System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] UpdateConnectionStatusFromManager: CurrentDevice={currentDevice?.Nome ?? "null"}, IsDeviceConnected={isDeviceConnected}, Conectado={currentDevice?.Conectado ?? false}");

                // Verificar múltiplas condições para garantir estado correto
                if (currentDevice != null && currentDevice.Conectado && isDeviceConnected)
                {
                    System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] UpdateConnectionStatusFromManager: Dispositivo conectado - {currentDevice.Nome}");

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
                        IsConnectedToRX = false;
                        IsConnectedToPlatforms = true;
                        ConnectionStatus = "Conectado ao dispositivo virtual";
                    }
                }
                else
                {
                    // Não há dispositivo conectado ou dispositivo está desconectado
                    System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] UpdateConnectionStatusFromManager: Nenhum dispositivo conectado ou dispositivo desconectado. Limpando estado...");

                    // Limpar seleção do dispositivo RX
                    if (SelectedRXDevice != null)
                    {
                        SelectedRXDevice.IsConnected = false;
                        SelectedRXDevice = null;
                    }

                    // Limpar referência de peripheral
                    _connectedPeripheral = null;

                    // Atualizar estados de conexão - FORCE UPDATE
                    IsConnectedToRX = false;
                    IsConnectedToPlatforms = false;
                    ConnectionStatus = "Não Conectado";

                    System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] UpdateConnectionStatusFromManager: Estado limpo - IsConnectedToRX: {IsConnectedToRX}, IsConnectedOrConnectingToRX: {IsConnectedOrConnectingToRX}");
                }
            });
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
        /// Comando para conectar a um dispositivo específico.
        /// </summary>
        [RelayCommand]
        private async Task ConnectToSpecificDeviceAsync(BleDevice device)
        {
            if (_disposed || device == null) return;

            try
            {
                IsConnectingToRX = true;
                SelectedRXDevice = device; // Set early so UI can show info while connecting
                ConnectionStatus = $"Conectando ao {device.Name}...";

                // Parar scan se estiver em andamento
                await StopScanAsync();

                // Verificar se já existe um peripheral
                if (device.Peripheral is not IPeripheral peripheral)
                {
                    ConnectionStatus = "Erro: Dispositivo não disponível para conexão";
                    return;
                }

                _connectionCancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _connectionCancellationTokenSource.Token;

                // Conectar ao dispositivo via WeightDeviceManager
                var success = await _deviceManager.ConnectToPhysicalRXAsync(device, peripheral);

                if (success && !cancellationToken.IsCancellationRequested)
                {
                    device.IsConnected = true;
                    SelectedRXDevice = device; // Redundant, but ensures UI is up-to-date
                    IsConnectedToRX = true;
                    IsDeviceListVisible = false;
                    ConnectionStatus = $"Conectado ao {device.Name}";
                    _connectedPeripheral = peripheral;

                    // Limpar a lista de dispositivos após conexão bem-sucedida
                    DiscoveredDevices.Clear();
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    ConnectionStatus = $"Falha ao conectar ao {device.Name}";
                }
            }
            catch (OperationCanceledException)
            {
                ConnectionStatus = "Conexão cancelada";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Erro: {ex.Message}";
            }
            finally
            {
                IsConnectingToRX = false;
                _connectionCancellationTokenSource?.Dispose();
                _connectionCancellationTokenSource = null;
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
            try
            {
                _scanSubscription?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao fazer dispose da scanSubscription: {ex.Message}");
            }
            finally
            {
                _scanSubscription = null;
            }
            
            await Task.CompletedTask.ConfigureAwait(false);
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
                SelectedRXDevice = null; // Clear device info on cancel

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
                    SelectedRXDevice = null; // Clear device info on disconnect
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

                // Usar o WeightDeviceManager para desconectar (que já atualiza IsConnectedToRX via UpdateConnectionStatusFromManager)
                await _deviceManager.DisconnectCurrentDeviceAsync();

                // Desconectar dispositivos TX/plataformas se necessário
                IsConnectedToPlatforms = false;

                // Verificar se ainda há peripheral conectado que precisa ser desconectado diretamente
                if (_connectedPeripheral != null)
                {
                    try
                    {
                        _connectedPeripheral.CancelConnection();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao cancelar conexão do peripheral: {ex.Message}");
                    }
                    finally
                    {
                        _connectedPeripheral = null;
                    }
                }

                ConnectionStatus = "Desconectado";
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

        /// <summary>
        /// Comando para incrementar o número da rede.
        /// </summary>
        [RelayCommand]
        private void IncrementNetwork()
        {
            if (SelectedNetworkNumber < 255)
                SelectedNetworkNumber++;
        }

        /// <summary>
        /// Comando para decrementar o número da rede.
        /// </summary>
        [RelayCommand]
        private void DecrementNetwork()
        {
            if (SelectedNetworkNumber > 1)
                SelectedNetworkNumber--;
        }

        /// <summary>
        /// Comando para incrementar o número de plataformas.
        /// </summary>
        [RelayCommand]
        private void IncrementPlatforms()
        {
            if (SelectedTotalPlatforms < 8)
                SelectedTotalPlatforms++;
        }

        /// <summary>
        /// Comando para decrementar o número de plataformas.
        /// </summary>
        [RelayCommand]
        private void DecrementPlatforms()
        {
            if (SelectedTotalPlatforms > 1)
                SelectedTotalPlatforms--;
        }

        /// <summary>
        /// Comando para alternar para o modo RX.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSwitchMode))]
        private async Task SwitchToRXModeAsync()
        {
            if (SelectedMode != ConnectionMode.RX)
            {
                SelectedMode = ConnectionMode.RX;

                // Se estiver conectado a plataformas, desconectar
                if (IsConnectedToPlatforms)
                {
                    await DisconnectAsync();
                }
            }
        }

        /// <summary>
        /// Comando para alternar para o modo TX.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSwitchMode))]
        private async Task SwitchToTXModeAsync()
        {
            if (SelectedMode != ConnectionMode.TX)
            {
                SelectedMode = ConnectionMode.TX;

                // Se estiver conectado ao RX, desconectar
                if (IsConnectedToRX)
                {
                    await DisconnectAsync();
                }
            }
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

                    // Limpar monitoramento do dispositivo atual
                    if (_currentlyMonitoredDevice != null)
                    {
                        _currentlyMonitoredDevice.PropertyChanged -= OnCurrentDevicePropertyChanged;
                        _currentlyMonitoredDevice = null;
                    }

                    // Limpar coleções de forma thread-safe
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            // Dispose de todos os BleDevices que implementam IDisposable
                            foreach (var device in DiscoveredDevices.OfType<IDisposable>())
                            {
                                try
                                {
                                    device.Dispose();
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Erro ao fazer dispose de device: {ex.Message}");
                                }
                            }

                            foreach (var platform in DiscoveredTXPlatforms.OfType<IDisposable>())
                            {
                                try
                                {
                                    platform.Dispose();
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Erro ao fazer dispose de platform: {ex.Message}");
                                }
                            }

                            DiscoveredDevices.Clear();
                            DiscoveredTXPlatforms.Clear();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro ao limpar coleções: {ex.Message}");
                        }
                    });

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
        /// Finalizador para garantir que os recursos sejam liberados mesmo se Dispose não for chamado.
        /// </summary>
        ~BluetoothViewModel()
        {
            Dispose(false);
        }

        private IDispositivoPesagem? _currentlyMonitoredDevice;

        /// <summary>
        /// Configura o monitoramento do dispositivo atual
        /// </summary>
        private void SetupCurrentDeviceMonitoring()
        {
            // Remover monitoramento do dispositivo anterior
            if (_currentlyMonitoredDevice != null)
            {
                _currentlyMonitoredDevice.PropertyChanged -= OnCurrentDevicePropertyChanged;
            }

            // Configurar monitoramento do novo dispositivo
            _currentlyMonitoredDevice = _deviceManager.CurrentDevice;
            if (_currentlyMonitoredDevice != null)
            {
                _currentlyMonitoredDevice.PropertyChanged += OnCurrentDevicePropertyChanged;
                System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] Configurado monitoramento para dispositivo: {_currentlyMonitoredDevice.Nome}");
            }
        }

        /// <summary>
        /// Manipulador para mudanças de propriedade do dispositivo atual
        /// </summary>
        private void OnCurrentDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDispositivoPesagem.Conectado))
            {
                System.Diagnostics.Debug.WriteLine($"[BluetoothViewModel] Dispositivo {((IDispositivoPesagem)sender!).Nome} - Conectado mudou para: {((IDispositivoPesagem)sender).Conectado}");
                UpdateConnectionStatusFromManager();
            }
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