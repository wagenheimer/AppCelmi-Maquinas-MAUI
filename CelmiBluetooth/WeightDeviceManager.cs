using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using CelmiBluetooth.Devices;
using CelmiBluetooth.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth
{
    /// <summary>
    /// Enum que define os estados detalhados de conex�o de um dispositivo.
    /// </summary>
    public enum ConnectionPhase
    {
        /// <summary>
        /// Dispositivo est� desconectado.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Dispositivo est� no processo de conex�o inicial.
        /// </summary>
        Connecting,

        /// <summary>
        /// Dispositivo est� conectado e lendo dados iniciais.
        /// </summary>
        ReadingInitialData,

        /// <summary>
        /// Dispositivo est� totalmente conectado e pronto.
        /// </summary>
        Connected
    }

    /// <summary>
    /// Gerencia dispositivos de pesagem e suas conex�es.
    /// </summary>
    public partial class WeightDeviceManager : ObservableObject
    {
        #region Private Fields
        private static WeightDeviceManager? _instance;
        private bool _isInitialized;
        #endregion

        #region Properties
        /// <summary>
        /// Obt�m a inst�ncia singleton do WeightDeviceManager.
        /// </summary>
        public static WeightDeviceManager Instance => _instance ??= new WeightDeviceManager();

        /// <summary>
        /// Dispositivo atualmente conectado.
        /// </summary>
        [ObservableProperty]
        private IDispositivoPesagem? _currentDevice;

        /// <summary>
        /// Estado atual da conex�o do dispositivo.
        /// </summary>
        [ObservableProperty]
        private ConnectionPhase _connectionPhase = ConnectionPhase.Disconnected;

        /// <summary>
        /// Lista de dispositivos detectados.
        /// </summary>
        public ObservableCollection<IDispositivoPesagem> AvailableDevices { get; } = new();

        /// <summary>
        /// Indica se um dispositivo est� conectado.
        /// </summary>
        public bool IsDeviceConnected => CurrentDevice != null && CurrentDevice.Conectado;

        /// <summary>
        /// Indica se o dispositivo est� completamente conectado e pronto.
        /// </summary>
        public bool IsConnected => ConnectionPhase == ConnectionPhase.Connected;

        /// <summary>
        /// Indica se um dispositivo est� em processo de conex�o ou leitura de dados.
        /// </summary>
        public bool IsConnecting => ConnectionPhase == ConnectionPhase.Connecting ||
                                    ConnectionPhase == ConnectionPhase.ReadingInitialData;

        /// <summary>
        /// Status da conex�o como texto para exibi��o.
        /// </summary>
        public string ConnectionStatusText => GetConnectionStatusText();
        #endregion

        #region Constructor
        private WeightDeviceManager()
        {
            // Construtor privado para padr�o singleton
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Inicializa o gerenciador de dispositivos.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                // Criar e adicionar dispositivo virtual para simula��o (sempre dispon�vel)
                var virtualRxDevice = VirtualRXDevice.Instance;
                AvailableDevices.Add(virtualRxDevice);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inicializar WeightDeviceManager: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Conecta a um dispositivo espec�fico.
        /// </summary>
        /// <param name="device">O dispositivo para conectar.</param>
        /// <returns>True se conectado com sucesso.</returns>
        public async Task<bool> ConnectDeviceAsync(IDispositivoPesagem device)
        {
            if (device == null)
                return false;

            try
            {
                // Desconectar do dispositivo atual se houver um
                if (CurrentDevice != null && CurrentDevice != device && CurrentDevice.Conectado)
                {
                    await DisconnectCurrentDeviceAsync();
                }

                // Atualizar fase de conex�o para "Conectando"
                ConnectionPhase = ConnectionPhase.Connecting;
                OnPropertyChanged(nameof(IsConnecting));
                OnPropertyChanged(nameof(ConnectionStatusText));

                // Atualizar dispositivo atual antes de conectar para que a UI j� mostre que est� conectando
                CurrentDevice = device;

                // Conectar ao novo dispositivo
                bool success = await device.ConectarAsync();

                if (success)
                {
                    // Atualizar para lendo dados iniciais
                    ConnectionPhase = ConnectionPhase.ReadingInitialData;
                    OnPropertyChanged(nameof(IsConnecting));
                    OnPropertyChanged(nameof(ConnectionStatusText));

                    // Iniciar leitura de peso se for necess�rio
                    await device.CriaTaskLeituraValoresNoIntervalo(CancellationToken.None);
                    
                    // Marcar como conectado apenas quando tiver lido todos os dados
                    ConnectionPhase = ConnectionPhase.Connected;
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(ConnectionStatusText));
                }
                else
                {
                    // Se falhou na conex�o, voltar ao estado desconectado
                    ConnectionPhase = ConnectionPhase.Disconnected;
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(IsConnecting));
                    OnPropertyChanged(nameof(ConnectionStatusText));
                }

                OnPropertyChanged(nameof(IsDeviceConnected));
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao conectar dispositivo: {ex.Message}");
                ConnectionPhase = ConnectionPhase.Disconnected;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(IsConnecting));
                OnPropertyChanged(nameof(ConnectionStatusText));
                return false;
            }
        }

        /// <summary>
        /// Desconecta do dispositivo atual.
        /// </summary>
        public async Task DisconnectCurrentDeviceAsync()
        {
            if (CurrentDevice == null)
                return;

            try
            {
                await CurrentDevice.DesconectarAsync();

                ConnectionPhase = ConnectionPhase.Disconnected;
                OnPropertyChanged(nameof(IsDeviceConnected));
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(IsConnecting));
                OnPropertyChanged(nameof(ConnectionStatusText));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao desconectar dispositivo: {ex.Message}");
                ConnectionPhase = ConnectionPhase.Disconnected;
                OnPropertyChanged(nameof(ConnectionStatusText));
            }
        }

        /// <summary>
        /// Adiciona um dispositivo � lista de dispon�veis.
        /// </summary>
        /// <param name="device">O dispositivo a adicionar.</param>
        public void AddDevice(IDispositivoPesagem device)
        {
            if (device == null)
                return;

            // Verificar se dispositivo j� existe na lista
            if (!AvailableDevices.Contains(device))
            {
                AvailableDevices.Add(device);
            }
        }

        /// <summary>
        /// Remove um dispositivo da lista de dispon�veis.
        /// </summary>
        /// <param name="device">O dispositivo a remover.</param>
        public void RemoveDevice(IDispositivoPesagem device)
        {
            if (device == null)
                return;

            // Desconectar se for o dispositivo atual
            if (CurrentDevice == device)
            {
                _ = DisconnectCurrentDeviceAsync();
                CurrentDevice = null;
                ConnectionPhase = ConnectionPhase.Disconnected;
                OnPropertyChanged(nameof(IsDeviceConnected));
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(IsConnecting));
                OnPropertyChanged(nameof(ConnectionStatusText));
            }

            AvailableDevices.Remove(device);
        }

        /// <summary>
        /// Obt�m texto de status para a fase atual de conex�o.
        /// </summary>
        public string GetConnectionStatusText()
        {
            return ConnectionPhase switch
            {
                ConnectionPhase.Connected => "Conectado",
                ConnectionPhase.Connecting => "Conectando",
                ConnectionPhase.ReadingInitialData => "Lendo Dados Iniciais",
                _ => "N�o Conectado"
            };
        }

        /// <summary>
        /// Verifica se � poss�vel executar comandos no dispositivo atual.
        /// </summary>
        /// <returns>True se o dispositivo est� completamente conectado e pronto para comandos.</returns>
        public bool CanExecuteCommands()
        {
            return ConnectionPhase == ConnectionPhase.Connected && CurrentDevice != null && CurrentDevice.Conectado;
        }

        /// <summary>
        /// Verifica o status da primeira leitura de dados.
        /// </summary>
        /// <returns>True se o dispositivo j� fez a primeira leitura completa dos dados.</returns>
        public bool HasCompletedInitialReading()
        {
            if (CurrentDevice is VirtualRXDevice virtualDevice)
            {
                return virtualDevice.JaLeuTodasPlataformas;
            }
            else if (CurrentDevice is PhysicalRXDevice physicalDevice)
            {
                return physicalDevice.JaLeuTodasPlataformas;
            }

            return false;
        }

        /// <summary>
        /// Cria e conecta um PhysicalRXDevice a partir de um BleDevice e IPeripheral.
        /// </summary>
        public async Task<bool> ConnectToPhysicalRXAsync(BleDevice bleDevice, Shiny.BluetoothLE.IPeripheral peripheral)
        {
            if (bleDevice == null || peripheral == null)
                return false;

            var physicalDevice = new CelmiBluetooth.Devices.PhysicalRXDevice(bleDevice, peripheral);
            AddDevice(physicalDevice);
            return await ConnectDeviceAsync(physicalDevice);
        }

        /// <summary>
        /// Conecta ao dispositivo virtual RX (singleton).
        /// </summary>
        public async Task<bool> ConnectToVirtualRXAsync()
        {
            var virtualDevice = CelmiBluetooth.Devices.VirtualRXDevice.Instance;
            AddDevice(virtualDevice);
            return await ConnectDeviceAsync(virtualDevice);
        }
        #endregion
    }
}