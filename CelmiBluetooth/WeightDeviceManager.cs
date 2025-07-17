using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using CelmiBluetooth.Devices;
using CelmiBluetooth.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth
{
    /// <summary>
    /// Enum que define os estados detalhados de conexão de um dispositivo.
    /// </summary>
    public enum ConnectionPhase
    {
        /// <summary>
        /// Dispositivo está desconectado.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Dispositivo está no processo de conexão inicial.
        /// </summary>
        Connecting,

        /// <summary>
        /// Dispositivo está conectado e lendo dados iniciais.
        /// </summary>
        ReadingInitialData,

        /// <summary>
        /// Dispositivo está totalmente conectado e pronto.
        /// </summary>
        Connected
    }

    /// <summary>
    /// Gerencia dispositivos de pesagem e suas conexões.
    /// </summary>
    public partial class WeightDeviceManager : ObservableObject
    {
        #region Private Fields
        private static WeightDeviceManager? _instance;
        private bool _isInitialized;
        #endregion

        #region Properties
        /// <summary>
        /// Obtém a instância singleton do WeightDeviceManager.
        /// </summary>
        public static WeightDeviceManager Instance => _instance ??= new WeightDeviceManager();

        /// <summary>
        /// Dispositivo atualmente conectado.
        /// </summary>
        [ObservableProperty]
        private IDispositivoPesagem? currentDevice;

        /// <summary>
        /// Estado atual da conexão do dispositivo.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDeviceConnected))]
        [NotifyPropertyChangedFor(nameof(IsConnected))]
        [NotifyPropertyChangedFor(nameof(IsConnecting))]
        [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
        private ConnectionPhase connectionPhase = ConnectionPhase.Disconnected;

        /// <summary>
        /// Lista de dispositivos detectados.
        /// </summary>
        public ObservableCollection<IDispositivoPesagem> AvailableDevices { get; } = new();

        /// <summary>
        /// Indica se um dispositivo está conectado.
        /// </summary>
        public bool IsDeviceConnected => CurrentDevice != null && CurrentDevice.Conectado;

        /// <summary>
        /// Indica se o dispositivo está completamente conectado e pronto.
        /// </summary>
        public bool IsConnected => ConnectionPhase == ConnectionPhase.Connected;

        /// <summary>
        /// Indica se um dispositivo está em processo de conexão ou leitura de dados.
        /// </summary>
        public bool IsConnecting => ConnectionPhase == ConnectionPhase.Connecting ||
                                    ConnectionPhase == ConnectionPhase.ReadingInitialData;

        /// <summary>
        /// Status da conexão como texto para exibição.
        /// </summary>
        public string ConnectionStatusText => GetConnectionStatusText();
        #endregion

        #region Constructor
        private WeightDeviceManager()
        {
            // Construtor privado para padrão singleton
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
                // Criar e adicionar dispositivo virtual para simulação (sempre disponível)
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
        /// Conecta a um dispositivo específico.
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

                // Atualizar fase de conexão para "Conectando"
                ConnectionPhase = ConnectionPhase.Connecting;

                // Atualizar dispositivo atual antes de conectar para que a UI já mostre que está conectando
                CurrentDevice = device;

                // Conectar ao novo dispositivo
                bool success = await device.ConectarAsync();

                if (success)
                {
                    // Atualizar para lendo dados iniciais
                    ConnectionPhase = ConnectionPhase.ReadingInitialData;

                    // Ler valores iniciais do dispositivo
                    await device.LerValoresIniciaisAsync();

                    // Iniciar leitura de peso contínua usando o novo método com Reactive
                    await device.IniciaLeituraValoresManuaisAsync();
                    
                    // Marcar como conectado apenas quando tiver lido todos os dados
                    ConnectionPhase = ConnectionPhase.Connected;
                }
                else
                {
                    // Se falhou na conexão, voltar ao estado desconectado
                    ConnectionPhase = ConnectionPhase.Disconnected;
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
            {
                System.Diagnostics.Debug.WriteLine("[WeightDeviceManager] DisconnectCurrentDeviceAsync: Nenhum dispositivo conectado para desconectar.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[WeightDeviceManager] DisconnectCurrentDeviceAsync: Iniciando desconexão do dispositivo '{CurrentDevice.Nome}'.");

            try
            {
                await CurrentDevice.DesconectarAsync();
                System.Diagnostics.Debug.WriteLine($"[WeightDeviceManager] DisconnectCurrentDeviceAsync: Dispositivo '{CurrentDevice.Nome}' desconectado com sucesso.");
                ConnectionPhase = ConnectionPhase.Disconnected;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WeightDeviceManager] DisconnectCurrentDeviceAsync: Erro ao desconectar dispositivo '{CurrentDevice.Nome}': {ex.Message}");
                ConnectionPhase = ConnectionPhase.Disconnected;
            }
            finally
            {
                // Limpar referência do dispositivo atual
                CurrentDevice = null;
                
                // Notificar ViewModel para atualizar status de conexão
                OnPropertyChanged(nameof(CurrentDevice));
                OnPropertyChanged(nameof(IsDeviceConnected));
                System.Diagnostics.Debug.WriteLine("[WeightDeviceManager] DisconnectCurrentDeviceAsync: CurrentDevice definido como null e notificações PropertyChanged enviadas para atualizar o BluetoothViewModel.");
            }
         }

        /// <summary>
        /// Adiciona um dispositivo à lista de disponíveis.
        /// </summary>
        /// <param name="device">O dispositivo a adicionar.</param>
        public void AddDevice(IDispositivoPesagem device)
        {
            if (device == null)
                return;

            // Verificar se dispositivo já existe na lista
            if (!AvailableDevices.Contains(device))
            {
                AvailableDevices.Add(device);
            }
        }

        /// <summary>
        /// Remove um dispositivo da lista de disponíveis.
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
            }

            AvailableDevices.Remove(device);
        }

        /// <summary>
        /// Obtém texto de status para a fase atual de conexão.
        /// </summary>
        public string GetConnectionStatusText()
        {
            return ConnectionPhase switch
            {
                ConnectionPhase.Connected => "Conectado",
                ConnectionPhase.Connecting => "Conectando",
                ConnectionPhase.ReadingInitialData => "Lendo Dados Iniciais",
                _ => "Não Conectado"
            };
        }

        /// <summary>
        /// Verifica se é possível executar comandos no dispositivo atual.
        /// </summary>
        /// <returns>True se o dispositivo está completamente conectado e pronto para comandos.</returns>
        public bool CanExecuteCommands()
        {
            return ConnectionPhase == ConnectionPhase.Connected && CurrentDevice != null && CurrentDevice.Conectado;
        }

        /// <summary>
        /// Verifica o status da primeira leitura de dados.
        /// </summary>
        /// <returns>True se o dispositivo já fez a primeira leitura completa dos dados.</returns>
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