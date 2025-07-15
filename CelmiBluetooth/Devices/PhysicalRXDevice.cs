using CelmiBluetooth.Utils;
using CelmiBluetooth.Models;
using CelmiBluetooth.Platforms;

using CommunityToolkit.Mvvm.ComponentModel;

using Shiny.BluetoothLE;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CelmiBluetooth.Devices
{
    /// <summary>
    /// Define tipos de dispositivo suportados
    /// </summary>
    public enum DeviceType
    {
        Unknown,
        PhysicalRX,
        VirtualRX
    }

    /// <summary>
    /// Define os possíveis status de uma plataforma
    /// </summary>
    public enum PlatformStatus
    {
        Unknown,
        Connected,
        Disconnected,
        Overloaded,
        Error
    }

    /// <summary>
    /// Argumentos para eventos de atualização de peso
    /// </summary>
    public class WeightUpdatedEventArgs : EventArgs
    {
        public int PlatformId { get; set; }
        public decimal Weight { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsStable { get; set; }
    }

    /// <summary>
    /// Argumentos para eventos de mudança de status de conexão
    /// </summary>
    public class ConnectionStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Dados de uma plataforma de pesagem
    /// </summary>
    public class ExtendedWeightData
    {
        public int PlatformId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CurrentWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public bool IsStable { get; set; }
        public DateTime LastUpdate { get; set; }
        public PlatformStatus Status { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Representa um dispositivo RX físico (visor Bluetooth).
    /// </summary>
    public partial class PhysicalRXDevice : BaseDispositivoPesagem
    {
        #region Private Fields
        private readonly IPeripheral _peripheral;
        private readonly BleDevice _celmiDevice;
        private CancellationTokenSource? _readingCancellationToken;
        private CancellationTokenSource? _connectionCancellationToken;
        private bool _disposed = false;
        private int _valoresSendoLidos = 0;
        private bool _jaLeuDadosGeral = false;
        private bool _jaTeveAlgumaPesagemNovaAposConectar = false;
        private bool _leituraEmAndamento = false;

        // RX Platform data - agora usando PlataformaRX
        private PlataformaRX? _geral;
        private PlataformaRX[] _plataformas = Array.Empty<PlataformaRX>();
        #endregion

        #region Implementação das Propriedades Abstratas
        /// <summary>
        /// Nome do dispositivo.
        /// </summary>
        public override string Nome => _celmiDevice.Name;

        /// <summary>
        /// Status da conexão do dispositivo.
        /// </summary>
        public override string Status => Conectado ? "Conectado" : "Desconectado";

        /// <summary>
        /// Tipo do dispositivo (sempre RXFisico).
        /// </summary>
        public override TipoDispositivo Tipo => TipoDispositivo.RXFisico;

        /// <summary>
        /// Número total de plataformas suportadas.
        /// </summary>
        public override int TotalPlataformas => _geral?.QuantidadePlataformas ?? _celmiDevice.Plataformas ?? 0;

        /// <summary>
        /// Peso total de todas as plataformas ativas.
        /// </summary>
        public override decimal PesoTotal => _geral != null ? (decimal)_geral.Peso : 0m;
        #endregion

        #region Public Properties
        /// <summary>
        /// ID único do dispositivo.
        /// </summary>
        public string DeviceId => _celmiDevice.Address;

        /// <summary>
        /// Nome do dispositivo.
        /// </summary>
        public string DeviceName => _celmiDevice.Name;

        /// <summary>
        /// Tipo do dispositivo (sempre PhysicalRX).
        /// </summary>
        public DeviceType Type => DeviceType.PhysicalRX;

        /// <summary>
        /// Indica se o dispositivo está conectado.
        /// </summary>
        public bool IsConnected => Conectado;

        /// <summary>
        /// Versão do firmware do dispositivo.
        /// </summary>
        public string FirmwareVersion
        {
            get => _celmiDevice.FirmwareVersion;
            set => _celmiDevice.FirmwareVersion = value;
        }

        /// <summary>
        /// Número total de plataformas suportadas.
        /// </summary>
        public int TotalPlatforms => TotalPlataformas;

        /// <summary>
        /// Peso total de todas as plataformas ativas.
        /// </summary>
        public decimal TotalWeight => PesoTotal;

        /// <summary>
        /// Lista de dados de peso das plataformas.
        /// </summary>
        public ObservableCollection<ExtendedWeightData> PlatformWeights { get; } = new();

        /// <summary>
        /// Dados gerais do RX.
        /// </summary>
        public PlataformaRX? Geral => _geral;

        /// <summary>
        /// Array das plataformas individuais.
        /// </summary>
        public PlataformaRX[] Plataformas => _plataformas;

        /// <summary>
        /// Indica se já leu todas as plataformas.
        /// </summary>
        public bool JaLeuTodasPlataformas => _plataformas.All(plat => plat != null && !string.IsNullOrEmpty(plat.Nome));

        /// <summary>
        /// Verifica se todas as plataformas estão aferidas.
        /// </summary>
        public bool TodosAferidos => _plataformas.All(plat => plat != null && plat.PesagemAferida);

        /// <summary>
        /// Verifica se todas têm carga viva habilitada.
        /// </summary>
        public bool TodosComCargaViva => _plataformas.All(plat => plat != null && plat.CargaVivaHabilitada);

        /// <summary>
        /// Verifica se todas têm carga viva fixada.
        /// </summary>
        public bool TodosComCargaVivaFixou => _plataformas.All(plat => plat != null && plat.CargaVivaFixou);

        /// <summary>
        /// Verifica se alguma plataforma precisa piscar (carga viva não fixada).
        /// </summary>
        public bool AlgumaPlataformaPrecisaPiscar => _plataformas.Any(plat => plat != null && plat.PrecisaPiscar);

        /// <summary>
        /// Verifica se alguma plataforma não está estável.
        /// </summary>
        public bool AlgumaPlataformaNaoEstavel => _plataformas.Any(plat => plat != null && !plat.PesagemEstavel);

        public string Name => DeviceName;

        /// <summary>
        /// Indica se o dispositivo está em processo de conexão.
        /// </summary>
        [ObservableProperty]
        private bool _isConnecting;
        #endregion

        #region Events
        /// <summary>
        /// Evento disparado quando os dados de peso são atualizados.
        /// </summary>
        public event EventHandler<WeightUpdatedEventArgs>? WeightUpdated;

        /// <summary>
        /// Evento disparado quando o status de conexão muda.
        /// </summary>
        public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// Construtor do dispositivo RX físico.
        /// </summary>
        /// <param name="celmiDevice">Dispositivo Celmi descoberto.</param>
        /// <param name="peripheral">Peripheral Bluetooth do Shiny.</param>
        public PhysicalRXDevice(BleDevice celmiDevice, IPeripheral peripheral)
        {
            _celmiDevice = celmiDevice;
            _peripheral = peripheral;
            InitializePlatforms();
            ExtractFirmwareVersion();
        }
        #endregion

        #region Implementação dos Métodos Abstratos
        /// <summary>
        /// Implementação específica de conexão do dispositivo físico.
        /// </summary>
        protected override async Task<bool> ConectarImplementacaoAsync(Dictionary<string, object>? parametros)
        {
            if (_disposed || Conectado)
                return Conectado;

            try
            {
                IsConnecting = true;
                OnPropertyChanged(nameof(IsConnecting));

                _connectionCancellationToken = new CancellationTokenSource();

                await _peripheral.ConnectAsync(
                    config: null,
                    cancelToken: _connectionCancellationToken.Token,
                    timeout: TimeSpan.FromSeconds(10)
                );

                if (_peripheral.Status == ConnectionState.Connected)
                {
                    ResetaValoresPadrao();
                    await LerValoresIniciaisAsync();
                    OnConnectionChanged(true);
                    return true;
                }
                else
                {
                    IsConnecting = false;
                    OnPropertyChanged(nameof(IsConnecting));
                }
            }
            catch (OperationCanceledException)
            {
                IsConnecting = false;
                OnPropertyChanged(nameof(IsConnecting));
                OnConnectionChanged(false, "Conexão cancelada ou tempo esgotado.");
            }
            catch (Exception ex)
            {
                IsConnecting = false;
                OnPropertyChanged(nameof(IsConnecting));
                OnConnectionChanged(false, ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Implementação específica de desconexão do dispositivo físico.
        /// </summary>
        protected override async Task DesconectarImplementacaoAsync()
        {
            if (_disposed || !Conectado)
                return;

            await StopWeightReadingAsync();

            try
            {
                _connectionCancellationToken?.Cancel();
                _peripheral.CancelConnection();
                IsConnecting = false;

                // Resetar dados e status das plataformas
                ResetaValoresPadrao();
                UpdatePlatformStatus(StatusPlataforma.Desconectada);

                OnPropertyChanged(nameof(IsConnecting));
                OnConnectionChanged(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao desconectar RX: {ex.Message}");
            }
        }

        /// <summary>
        /// Implementação específica de tara de plataforma.
        /// </summary>
        protected override async Task TararPlataformaImplementacaoAsync(int idPlataforma)
        {
            if (!Conectado)
                return;

            try
            {
                // Enviar comando de tara para o dispositivo
                await EnviarComandoAsync(CelmiUUID.Characteristic.CMD_TARA);

                // Atualizar dados locais
                var platform = PlatformWeights.FirstOrDefault(p => p.PlatformId == idPlataforma);
                if (platform != null)
                {
                    platform.TareWeight = platform.GrossWeight;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao tarar plataforma {idPlataforma}: {ex.Message}");
            }
        }

        /// <summary>
        /// Implementação específica de zero de plataforma.
        /// </summary>
        protected override async Task ZerarPlataformaImplementacaoAsync(int idPlataforma)
        {
            if (!Conectado)
                return;

            try
            {
                await EnviarComandoAsync(CelmiUUID.Characteristic.CMD_ZERO);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao zerar plataforma {idPlataforma}: {ex.Message}");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Para a leitura contínua dos pesos.
        /// </summary>
        public async Task StopWeightReadingAsync()
        {
            _readingCancellationToken?.Cancel();
            _readingCancellationToken?.Dispose();
            _readingCancellationToken = null;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Zera/tara uma plataforma específica.
        /// </summary>
        public async Task TarePlatformAsync(int platformId)
        {
            await TararPlataformaAsync(platformId);
        }

        /// <summary>
        /// Zera/tara todas as plataformas.
        /// </summary>
        public async Task TareAllPlatformsAsync()
        {
            await TararTodasPlataformasAsync();
        }

        /// <summary>
        /// Realiza zero em todas as plataformas.
        /// </summary>
        public async Task ZeroAllPlatformsAsync()
        {
            await ZerarTodasPlataformasAsync();
        }

        /// <summary>
        /// Obtém dados de uma plataforma específica.
        /// </summary>
        public PlataformaRX? PegaPlataforma(int plataforma)
        {
            if (plataforma > 0 && plataforma <= _plataformas.Length)
                return _plataformas[plataforma - 1];
            return null;
        }

        /// <summary>
        /// Obtém peso formatado de uma plataforma.
        /// </summary>
        public string GetPesoFormatado(int plataforma)
        {
            var platforma = PegaPlataforma(plataforma);
            if (platforma == null)
                return "Desconectado";

            return FormatarPeso(platforma.Peso, platforma.CasasDecimais, platforma.TipoViolacao,
                Conectado, platforma.PesagemAferida, platforma.CargaVivaHabilitada,
                platforma.CargaVivaFixou, platforma.PesagemEstavel);
        }

        /// <summary>
        /// Obtém peso total formatado.
        /// </summary>
        public string GetPesoTotalFormatado()
        {
            if (_geral == null)
                return "Conectando";

            return FormatarPeso(_geral.Peso, _geral.CasasDecimais, GetAlgumaComViolacao(),
                Conectado, TodosAferidos, TodosComCargaViva, TodosComCargaVivaFixou,
                !AlgumaPlataformaNaoEstavel);
        }

        /// <summary>
        /// Implementação da interface IDispositivoPesagem - Obtém peso total formatado.
        /// </summary>
        public override string ObterPesoTotalFormatado()
        {
            return GetPesoTotalFormatado();
        }

        /// <summary>
        /// Obtém peso formatado de plataforma.
        /// </summary>
        public override string ObterPesoPlataformaFormatado(int idPlataforma)
        {
            return GetPesoFormatado(idPlataforma);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Extrai a versão do firmware do nome do dispositivo.
        /// </summary>
        private void ExtractFirmwareVersion()
        {
            try
            {
                var firmwareVersion = CelmiDeviceUtils.ExtractFirmwareVersion(DeviceName);
                ((BleDevice)_celmiDevice).FirmwareVersion = !string.IsNullOrEmpty(firmwareVersion) ? firmwareVersion : "F000";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao extrair versão do firmware: {ex.Message}");
                ((BleDevice)_celmiDevice).FirmwareVersion = "F000"; // Versão padrão em caso de erro
            }
        }

        /// <summary>
        /// Inicializa as plataformas disponíveis no dispositivo.
        /// </summary>
        private void InitializePlatforms()
        {
            PlatformWeights.Clear();
            var initialPlatformCount = _celmiDevice.Plataformas ?? 1;

            for (int i = 1; i <= initialPlatformCount; i++)
            {
                PlatformWeights.Add(new ExtendedWeightData
                {
                    PlatformId = i,
                    Description = $"Plataforma {i}",
                    Status = PlatformStatus.Disconnected
                });
            }
        }

        /// <summary>
        /// Reseta valores para padrão inicial.
        /// </summary>
        private void ResetaValoresPadrao()
        {
            _jaTeveAlgumaPesagemNovaAposConectar = false;
            _jaLeuDadosGeral = false;
            _geral = null;
            _plataformas = Array.Empty<PlataformaRX>();
            _valoresSendoLidos = 0;
        }

        /// <summary>
        /// Atualiza o status de todas as plataformas.
        /// </summary>
        private void UpdatePlatformStatus(StatusPlataforma status)
        {
            AtualizarStatusPlataformas(status);

            foreach (var platform in PlatformWeights)
            {
                platform.Status = status switch
                {
                    StatusPlataforma.Conectada => PlatformStatus.Connected,
                    StatusPlataforma.Desconectada => PlatformStatus.Disconnected,
                    StatusPlataforma.Sobrecarga => PlatformStatus.Overloaded,
                    _ => PlatformStatus.Unknown
                };
                platform.IsActive = status == StatusPlataforma.Conectada;
            }
        }

        /// <summary>
        /// Inicia leitura periódica dos valores.
        /// </summary>
        private async Task IniciaLeituraValoresManuaisAsync()
        {
            var token = _readingCancellationToken?.Token ?? CancellationToken.None;
            while (!token.IsCancellationRequested && Conectado && !_disposed)
            {
                if (_leituraEmAndamento)
                {
                    await Task.Delay(IntervaloLeitura, token);
                    continue;
                }

                try
                {
                    _leituraEmAndamento = true;
                    await LerValoresManuaisNoIntervalo();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro na leitura periódica: {ex.Message}");
                    _leituraEmAndamento = false;
                    await Task.Delay(2000, token); // Aguarda antes de tentar novamente
                    continue;
                }
                finally
                {
                    _leituraEmAndamento = false;
                }

                await Task.Delay(IntervaloLeitura, token);
            }
        }

        /// <summary>
        /// Lê valores iniciais do dispositivo.
        /// </summary>
        private async Task LerValoresIniciaisAsync()
        {
            try
            {
                // Ler a versão do firmware primeiro
                await LerFirmwareVersionAsync();

                // Depois lê dados gerais
                await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXGeral);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao ler valores iniciais: {ex.Message}");
            }
        }

        /// <summary>
        /// Lê a versão do firmware do dispositivo.
        /// </summary>
        private async Task LerFirmwareVersionAsync()
        {
            if (_peripheral == null || !Conectado)
                return;

            ExtractFirmwareVersion();

        }

        /// <summary>
        /// Leitura manual das notificações.
        /// </summary>
        private async Task LerValoresManuaisNoIntervalo()
        {
            if (_disposed || !Conectado)
                return;

            try
            {
                // Sempre ler dados gerais
                await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXGeral);

                // Ler plataformas individuais se já leu dados gerais
                if (_geral != null)
                {
                    await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat1);

                    if (_geral.QuantidadePlataformas > 1) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat2);
                    if (_geral.QuantidadePlataformas > 2) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat3);
                    if (_geral.QuantidadePlataformas > 3) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat4);
                    if (_geral.QuantidadePlataformas > 4) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat5);
                    if (_geral.QuantidadePlataformas > 5) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat6);
                    if (_geral.QuantidadePlataformas > 6) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat7);
                    if (_geral.QuantidadePlataformas > 7) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat8);
                    if (_geral.QuantidadePlataformas > 8) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat9);
                    if (_geral.QuantidadePlataformas > 9) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat10);
                    if (_geral.QuantidadePlataformas > 10) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat11);
                    if (_geral.QuantidadePlataformas > 11) await LerValorAsync(CelmiUUID.Service.DISPOSITIVORX, CelmiUUID.Characteristic.RXPlat12);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na leitura manual: {ex.Message}");
            }
        }

        /// <summary>
        /// Lê um valor específico de uma característica.
        /// </summary>
        private async Task LerValorAsync(string servico, string caracteristica)
        {
            if (_peripheral == null || !Conectado || _valoresSendoLidos > 30)
                return;

            _valoresSendoLidos++;

            try
            {
                var result = await _peripheral.ReadCharacteristicAsync(servico, caracteristica, _connectionCancellationToken?.Token ?? CancellationToken.None);
                ProcessarValorCaracteristica(caracteristica, result?.Data);
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Operação cancelada.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao ler característica {caracteristica}: {ex.Message}");
            }
            finally
            {
                _valoresSendoLidos--;
            }
        }

        /// <summary>
        /// Processa dados recebidos das características.
        /// </summary>
        private void ProcessarValorCaracteristica(string uuid, byte[]? rawValue)
        {
            if (rawValue == null || rawValue.Length == 0)
                return;

            try
            {
                switch (uuid)
                {
                    case CelmiUUID.Characteristic.RXGeral:
                        if (DesmembraValores(rawValue, 0))
                            AtualizaUI();
                        break;

                    case CelmiUUID.Characteristic.RXPlat1:
                        if (DesmembraValores(rawValue, 1))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat2:
                        if (DesmembraValores(rawValue, 2))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat3:
                        if (DesmembraValores(rawValue, 3))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat4:
                        if (DesmembraValores(rawValue, 4))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat5:
                        if (DesmembraValores(rawValue, 5))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat6:
                        if (DesmembraValores(rawValue, 6))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat7:
                        if (DesmembraValores(rawValue, 7))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat8:
                        if (DesmembraValores(rawValue, 8))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat9:
                        if (DesmembraValores(rawValue, 9))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat10:
                        if (DesmembraValores(rawValue, 10))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat11:
                        if (DesmembraValores(rawValue, 11))
                            AtualizaUI();
                        break;
                    case CelmiUUID.Characteristic.RXPlat12:
                        if (DesmembraValores(rawValue, 12))
                            AtualizaUI();
                        break;
                }

                // Notificação de Carga Viva
                if (_jaTeveAlgumaPesagemNovaAposConectar && TodosComCargaVivaFixou)
                {
                    NotificarCargaViva();
                }
                else if (JaLeuTodasPlataformas && !TodosComCargaVivaFixou)
                {
                    _jaTeveAlgumaPesagemNovaAposConectar = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar característica {uuid}: {ex.Message}");
            }
        }

        /// <summary>
        /// Desmembra valores recebidos do RX.
        /// </summary>
        private bool DesmembraValores(byte[] rawValue, int plataforma = 0)
        {
            if (rawValue == null || rawValue.Length < 14)
                return false;

            try
            {
                if (plataforma == 0) // Dados gerais
                {
                    _geral ??= new PlataformaRX("Dados Gerais");
                    bool teveMudanca = _geral.ProcessarDadosRecebidos(rawValue, 0);

                    // Check if the platform count has changed
                    if (_plataformas.Length != _geral.QuantidadePlataformas)
                    {
                        // Resize the platforms array to match the new count
                        var oldPlatforms = _plataformas;
                        _plataformas = new PlataformaRX[_geral.QuantidadePlataformas];

                        // Copy existing platform data to preserve it
                        for (int i = 0; i < Math.Min(oldPlatforms.Length, _plataformas.Length); i++)
                        {
                            _plataformas[i] = oldPlatforms[i];
                        }

                        // Update the UI collection to match
                        AtualizarLayoutBalanca();
                        teveMudanca = true;
                    }

                    _jaLeuDadosGeral = true;
                    return teveMudanca;
                }
                else // Plataforma individual
                {
                    if (!_jaLeuDadosGeral || _geral == null || plataforma > _geral.QuantidadePlataformas)
                        return false;

                    if (_plataformas[plataforma - 1] == null)
                    {
                        _plataformas[plataforma - 1] = new PlataformaRX($"RX Plat {plataforma}");
                    }

                    return _plataformas[plataforma - 1].ProcessarDadosRecebidos(rawValue, plataforma);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao desmembrar valores: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Atualiza UI com os novos dados.
        /// </summary>
        private void AtualizaUI()
        {
            try
            {
                // Notify that TotalPlatforms may have changed if we just got RXGeral data
                if (_jaLeuDadosGeral && _geral != null)
                {
                    OnPropertyChanged(nameof(TotalPlatforms));
                    OnPropertyChanged(nameof(TotalPlataformas));
                }

                // Se já temos dados gerais e já lemos todas as plataformas, podemos desativar a animação de conexão
                if (_jaLeuDadosGeral && JaLeuTodasPlataformas && IsConnecting)
                {
                    IsConnecting = false;
                    OnPropertyChanged(nameof(IsConnecting));
                }

                // Atualizar peso total
                OnPropertyChanged(nameof(TotalWeight));
                OnPropertyChanged(nameof(PesoTotal));

                // Atualizar dados das plataformas
                AtualizarDadosPlataformas();

                // Disparar evento de atualização
                if (_geral != null)
                {
                    WeightUpdated?.Invoke(this, new WeightUpdatedEventArgs
                    {
                        PlatformId = 0, // 0 para total
                        Weight = TotalWeight,
                        Timestamp = DateTime.Now,
                        IsStable = !AlgumaPlataformaNaoEstavel
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza dados das plataformas na coleção.
        /// </summary>
        private void AtualizarDadosPlataformas()
        {
            for (int i = 0; i < _plataformas.Length && i < PlatformWeights.Count; i++)
            {
                var plataforma = _plataformas[i];
                var weightData = PlatformWeights[i];

                if (plataforma != null)
                {
                    weightData.CurrentWeight = (decimal)plataforma.Peso;
                    weightData.GrossWeight = (decimal)plataforma.PesoBruto;
                    weightData.IsStable = plataforma.PesagemEstavel;
                    weightData.LastUpdate = DateTime.Now;
                    weightData.Status = plataforma.Conectada ? PlatformStatus.Connected : PlatformStatus.Disconnected;
                    weightData.IsActive = plataforma.Conectada;

                    // Verificar sobrecarga
                    if (plataforma.Sobrecarga)
                        weightData.Status = PlatformStatus.Overloaded;

                    // Atualizar na coleção base também
                    AtualizarDadosPlataforma(i + 1, (decimal)plataforma.Peso, (decimal)plataforma.PesoBruto,
                        plataforma.PesagemEstavel, plataforma.TipoViolacao,
                        plataforma.Conectada ? StatusPlataforma.Conectada : StatusPlataforma.Desconectada);
                }
            }
        }

        /// <summary>
        /// Atualiza layout da balança quando muda número de plataformas.
        /// </summary>
        private void AtualizarLayoutBalanca()
        {
            // Get the current total from _geral if available, otherwise use the fallback
            int currentTotalPlatforms = _geral != null ? _geral.QuantidadePlataformas : (_celmiDevice.Plataformas ?? 0);

            // If the PlatformWeights collection doesn't match the total platforms count
            if (PlatformWeights.Count != currentTotalPlatforms)
            {
                // If we need to add more platforms
                if (PlatformWeights.Count < currentTotalPlatforms)
                {
                    for (int i = PlatformWeights.Count + 1; i <= currentTotalPlatforms; i++)
                    {
                        PlatformWeights.Add(new ExtendedWeightData
                        {
                            PlatformId = i,
                            Description = $"Plataforma {i}",
                            Status = PlatformStatus.Disconnected
                        });
                    }
                }
                // If we need to remove platforms
                else if (PlatformWeights.Count > currentTotalPlatforms)
                {
                    while (PlatformWeights.Count > currentTotalPlatforms)
                    {
                        PlatformWeights.RemoveAt(PlatformWeights.Count - 1);
                    }
                }

                // Update base collection as well
                InicializarPlataformas();

                // Notify that TotalPlatforms may have changed
                OnPropertyChanged(nameof(TotalPlatforms));
                OnPropertyChanged(nameof(TotalPlataformas));
            }
        }

        /// <summary>
        /// Envia comando para o dispositivo.
        /// </summary>
        private async Task<bool> EnviarComandoAsync(string comando)
        {
            if (!Conectado || _peripheral == null)
                return false;

            try
            {
                byte[] valueToSend = { 1 };

                _peripheral.WriteCharacteristic(CelmiUUID.Service.SERVICO_DE_COMANDOS, comando, valueToSend)
                    .Subscribe(
                        result => System.Diagnostics.Debug.WriteLine($"Comando {comando} enviado com sucesso"),
                        error => System.Diagnostics.Debug.WriteLine($"Erro ao enviar comando {comando}: {error.Message}"),
                        _connectionCancellationToken?.Token ?? CancellationToken.None
                    );

                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar comando {comando}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Notifica carga viva fixada.
        /// </summary>
        private void NotificarCargaViva()
        {
            try
            {
                // Aqui você pode implementar vibração, som ou outra notificação
                System.Diagnostics.Debug.WriteLine("Carga Viva Fixou - Notificação");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na notificação de carga viva: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém a primeira violação encontrada.
        /// </summary>
        private TipoViolacaoPeso GetAlgumaComViolacao()
        {
            if (_plataformas.Length == 0)
                return TipoViolacaoPeso.Normal;

            foreach (var plat in _plataformas)
            {
                if (plat != null && plat.TipoViolacao != TipoViolacaoPeso.Normal)
                    return plat.TipoViolacao;
            }

            return TipoViolacaoPeso.Normal;
        }

        /// <summary>
        /// Formata peso para exibição.
        /// </summary>
        private string FormatarPeso(float peso, int casasDecimais, TipoViolacaoPeso violacao,
            bool conectado, bool aferido, bool cargaVivaHabilitada, bool cargaVivaFixou, bool estavel)
        {
            if (!conectado)
                return "Desconectado";

            if (!aferido)
                return "Conectando";

            if (cargaVivaHabilitada && !cargaVivaFixou && !estavel)
                return "- - - - - -";

            var formato = $"F{casasDecimais}";
            var pesoFormatado = peso.ToString(formato);

            return violacao switch
            {
                TipoViolacaoPeso.Sobrecarga => $"++{pesoFormatado}",
                TipoViolacaoPeso.Subcarga => $"--{pesoFormatado}",
                _ => $"{pesoFormatado} kg"
            };
        }

        /// <summary>
        /// Dispara evento de mudança de conexão.
        /// </summary>
        private void OnConnectionChanged(bool isConnected, string? errorMessage = null)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs
            {
                IsConnected = isConnected,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.Now
            });
        }
        #endregion

        #region Sobrescrita de Dispose
        /// <summary>
        /// Libera recursos específicos do dispositivo físico.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _readingCancellationToken?.Cancel();
                    _readingCancellationToken?.Dispose();
                    _readingCancellationToken = null;
                    _connectionCancellationToken?.Cancel();
                    _connectionCancellationToken?.Dispose();
                    _connectionCancellationToken = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro durante dispose do PhysicalRXDevice: {ex.Message}");
                }
                finally
                {
                    _disposed = true;
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}