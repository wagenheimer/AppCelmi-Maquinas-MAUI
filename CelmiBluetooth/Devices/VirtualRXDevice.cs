using CommunityToolkit.Mvvm.ComponentModel;
using CelmiBluetooth.Models;
using CelmiBluetooth.Platforms;
using System.Collections.ObjectModel;
using Timer = System.Timers.Timer;

namespace CelmiBluetooth.Devices
{
    /// <summary>
    /// Implementa��o de um dispositivo virtual RX que simula dados de pesagem.
    /// Fornece uma simula��o real�stica para testes e desenvolvimento.
    /// </summary>
    public partial class VirtualRXDevice : BaseDispositivoPesagem
    {
        #region Padr�o Singleton
        private static VirtualRXDevice? _instancia;

        /// <summary>
        /// Obt�m a inst�ncia singleton do dispositivo virtual.
        /// </summary>
        public static VirtualRXDevice Instance => _instancia ??= new VirtualRXDevice();
        #endregion

        #region Campos Privados
        private readonly Timer _timerSimulacao;
        private readonly Random _geradorAleatorio = new();
        private PlataformaRX? _dadosGerais;
        private PlataformaRX[] _plataformas = [];
        private bool _jaLeuDadosGeral;
        private bool _jaTeveAlgumaPesagemNovaAposConectar;
        #endregion

        #region Implementa��o das Propriedades Abstratas
        /// <summary>
        /// Nome do dispositivo virtual.
        /// </summary>
        public override string Nome => "Dispositivo RX Virtual";

        /// <summary>
        /// Status da conex�o do dispositivo.
        /// </summary>
        public override string Status => Conectado ? "Conectado (Virtual)" : "Desconectado";

        /// <summary>
        /// Tipo do dispositivo (sempre RXVirtual).
        /// </summary>
        public override TipoDispositivo Tipo => TipoDispositivo.RXVirtual;

        /// <summary>
        /// N�mero total de plataformas suportadas.
        /// </summary>
        public override int TotalPlataformas => _dadosGerais?.QuantidadePlataformas ?? 4;

        /// <summary>
        /// Peso total de todas as plataformas ativas.
        /// </summary>
        public override decimal PesoTotal => _dadosGerais != null ? (decimal)_dadosGerais.Peso : 0m;
        #endregion

        #region Propriedades P�blicas
        /// <summary>
        /// Dados gerais do dispositivo RX.
        /// </summary>
        public PlataformaRX? Geral => _dadosGerais;

        /// <summary>
        /// Array das plataformas individuais.
        /// </summary>
        public ReadOnlySpan<PlataformaRX> Plataformas => _plataformas.AsSpan();

        /// <summary>
        /// Intervalo de simula��o em milissegundos.
        /// </summary>
        [ObservableProperty]
        private int _intervaloSimulacao = 1000;

        /// <summary>
        /// Indica se a simula��o est� ativa.
        /// </summary>
        [ObservableProperty]
        private bool _simulacaoAtiva;

        /// <summary>
        /// Indica se j� leu todas as plataformas.
        /// </summary>
        public bool JaLeuTodasPlataformas => _plataformas.All(plat => plat != null && !string.IsNullOrEmpty(plat.Nome));

        /// <summary>
        /// Verifica se todas as plataformas est�o aferidas.
        /// </summary>
        public bool TodosAferidos => _plataformas.All(plat => plat != null && plat.PesagemAferida);

        /// <summary>
        /// Verifica se todas t�m carga viva habilitada.
        /// </summary>
        public bool TodosComCargaViva => _plataformas.All(plat => plat != null && plat.CargaVivaHabilitada);

        /// <summary>
        /// Verifica se todas t�m carga viva fixada.
        /// </summary>
        public bool TodosComCargaVivaFixou => _plataformas.All(plat => plat != null && plat.CargaVivaFixou);

        /// <summary>
        /// Verifica se alguma plataforma precisa piscar (carga viva n�o fixada).
        /// </summary>
        public bool AlgumaPlataformaPrecisaPiscar => _plataformas.Any(plat => plat != null && plat.PrecisaPiscar);

        /// <summary>
        /// Verifica se alguma plataforma n�o est� est�vel.
        /// </summary>
        public bool AlgumaPlataformaNaoEstavel => _plataformas.Any(plat => plat != null && !plat.PesagemEstavel);

        /// <summary>
        /// Para compatibilidade
        /// </summary>
        public bool IsConnected => Conectado;

        /// <summary>
        /// Para compatibilidade
        /// </summary>
        public string Name => Nome;

        /// <summary>
        /// Vers�o do firmware do dispositivo virtual.
        /// </summary>
        public string FirmwareVersion { get; set; } = "FVirtual";
        #endregion

        #region Construtor
        /// <summary>
        /// Inicializa uma nova inst�ncia do dispositivo virtual RX.
        /// </summary>
        private VirtualRXDevice()
        {
            InicializarDadosGerais();
            ResetarValoresPadrao();

            // Configurar timer para simula��o
            _timerSimulacao = new Timer(IntervaloSimulacao);
            _timerSimulacao.Elapsed += (s, e) => ExecutarSimulacao();
        }
        #endregion

        #region Implementa��o dos M�todos Abstratos
        /// <summary>
        /// Implementa��o espec�fica de conex�o do dispositivo virtual.
        /// </summary>
        protected override async Task<bool> ConectarImplementacaoAsync(Dictionary<string, object>? parametros)
        {
            try
            {
                // Simular processo de conex�o
                await Task.Delay(100);

                InicializarPlataformas();
                ExecutarSimulacao(); // Simula��o inicial

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao conectar dispositivo virtual: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Implementa��o espec�fica de desconex�o do dispositivo virtual.
        /// </summary>
        protected override async Task DesconectarImplementacaoAsync()
        {
            try
            {
                _timerSimulacao.Stop();
                SimulacaoAtiva = false;
                AtualizarStatusPlataformas(StatusPlataforma.Desconectada);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao desconectar dispositivo virtual: {ex.Message}");
            }
        }

        /// <summary>
        /// Implementa��o espec�fica de leitura de peso cont�nua.
        /// </summary>
        protected async Task IniciaLeituraValoresManuaisAsync(CancellationToken cancellationToken)
        {
            try
            {
                _timerSimulacao.Interval = IntervaloLeitura;
                _timerSimulacao.Start();
                SimulacaoAtiva = true;

                // Manter a tarefa viva enquanto n�o for cancelada
                while (!cancellationToken.IsCancellationRequested && Conectado)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelamento esperado
            }
            finally
            {
                _timerSimulacao.Stop();
                SimulacaoAtiva = false;
            }
        }

        /// <summary>
        /// Implementa��o espec�fica de tara de plataforma.
        /// </summary>
        protected override async Task TararPlataformaImplementacaoAsync(int idPlataforma)
        {
            if (idPlataforma < 1 || idPlataforma > _plataformas.Length)
                return;

            try
            {
                var plataforma = _plataformas[idPlataforma - 1];
                plataforma?.AplicarTara();

                // Simular delay de comando
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao tarar plataforma virtual {idPlataforma}: {ex.Message}");
            }
        }

        /// <summary>
        /// Implementa��o espec�fica de zero de plataforma.
        /// </summary>
        protected override async Task ZerarPlataformaImplementacaoAsync(int idPlataforma)
        {
            if (idPlataforma < 1 || idPlataforma > _plataformas.Length)
                return;

            try
            {
                var plataforma = _plataformas[idPlataforma - 1];
                plataforma?.AplicarZero();

                // Simular delay de comando
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao zerar plataforma virtual {idPlataforma}: {ex.Message}");
            }
        }
        #endregion

        #region M�todos P�blicos
        /// <summary>
        /// Obt�m uma plataforma pelo �ndice.
        /// </summary>
        /// <param name="indice">�ndice da plataforma (0-based).</param>
        /// <returns>A plataforma ou null se n�o encontrada.</returns>
        public PlataformaRX? ObterPlataforma(int indice)
        {
            if (indice < 0 || indice >= _plataformas.Length)
                return null;

            return _plataformas[indice];
        }

        /// <summary>
        /// Obt�m uma plataforma pelo ID.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma (1-based).</param>
        /// <returns>A plataforma ou null se n�o encontrada.</returns>
        public PlataformaRX? ObterPlataformaPorId(int idPlataforma)
        {
            return ObterPlataforma(idPlataforma - 1);
        }

        /// <summary>
        /// Obt�m peso formatado de uma plataforma.
        /// </summary>
        public string GetFormattedWeight(int platformIndex)
        {
            var plataforma = ObterPlataforma(platformIndex);
            return plataforma?.ObterPesoFormatado() ?? "Desconectado";
        }

        /// <summary>
        /// Obt�m peso total formatado.
        /// </summary>
        public string GetFormattedTotalWeight()
        {
            return ObterPesoTotalFormatado();
        }

        /// <summary>
        /// Implementa��o da interface - Obt�m peso total formatado.
        /// </summary>
        public override string ObterPesoTotalFormatado()
        {
            if (_dadosGerais == null)
                return "Conectando";

            return _dadosGerais.ObterPesoFormatado();
        }

        /// <summary>
        /// Obt�m peso formatado de plataforma.
        /// </summary>
        public override string ObterPesoPlataformaFormatado(int idPlataforma)
        {
            var plataforma = ObterPlataformaPorId(idPlataforma);
            return plataforma?.ObterPesoFormatado() ?? "N�o encontrado";
        }

        /// <summary>
        /// Configura o n�mero de plataformas do dispositivo virtual.
        /// </summary>
        /// <param name="quantidade">Quantidade de plataformas (1-12).</param>
        public void ConfigurarQuantidadePlataformas(int quantidade)
        {
            if (quantidade < 1 || quantidade > 12)
                throw new ArgumentOutOfRangeException(nameof(quantidade), "Quantidade deve estar entre 1 e 12");

            if (_dadosGerais != null)
            {
                _dadosGerais.QuantidadePlataformas = quantidade;
                InicializarArrayPlataformas();

                if (Conectado)
                {
                    InicializarPlataformas();
                }
            }
        }

        /// <summary>
        /// Configura o intervalo de simula��o.
        /// </summary>
        /// <param name="intervaloMs">Intervalo em milissegundos.</param>
        public void ConfigurarIntervaloSimulacao(int intervaloMs)
        {
            if (intervaloMs < 100)
                throw new ArgumentOutOfRangeException(nameof(intervaloMs), "Intervalo m�nimo � 100ms");

            IntervaloSimulacao = intervaloMs;

            if (SimulacaoAtiva)
            {
                _timerSimulacao.Interval = intervaloMs;
            }
        }
        #endregion

        #region M�todos Privados
        /// <summary>
        /// Inicializa os dados gerais do dispositivo.
        /// </summary>
        private void InicializarDadosGerais()
        {
            _dadosGerais = new PlataformaRX("Dados Gerais")
            {
                QuantidadePlataformas = 4,
                Conectada = false,
                PesagemAferida = true,
                CasasDecimais = 2
            };
        }

        /// <summary>
        /// Inicializa o array de plataformas.
        /// </summary>
        private void InicializarArrayPlataformas()
        {
            var quantidadeAtual = _dadosGerais?.QuantidadePlataformas ?? 4;
            var plataformasAntigas = _plataformas;

            _plataformas = new PlataformaRX[quantidadeAtual];

            // Preservar dados existentes
            for (int i = 0; i < Math.Min(plataformasAntigas.Length, _plataformas.Length); i++)
            {
                _plataformas[i] = plataformasAntigas[i];
            }

            // Criar novas plataformas se necess�rio
            for (int i = plataformasAntigas.Length; i < _plataformas.Length; i++)
            {
                _plataformas[i] = new PlataformaRX($"Plataforma {i + 1}");
            }
        }

        /// <summary>
        /// Reseta os valores para o padr�o.
        /// </summary>
        private void ResetarValoresPadrao()
        {
            _jaTeveAlgumaPesagemNovaAposConectar = false;
            _jaLeuDadosGeral = false;

            InicializarArrayPlataformas();

            foreach (var plataforma in _plataformas)
            {
                plataforma?.ResetarValoresPadrao();
            }
        }

        /// <summary>
        /// Executa a simula��o de dados.
        /// </summary>
        private void ExecutarSimulacao()
        {
            if (!Conectado || _dadosGerais == null)
                return;

            try
            {
                // Simular dados gerais
                var dadosGeraisSimulados = GerarDadosSimulados(0);
                if (_dadosGerais.ProcessarDadosRecebidos(dadosGeraisSimulados, 0))
                {
                    _jaLeuDadosGeral = true;
                    NotificarPesoAtualizado(0, PesoTotal, _dadosGerais.PesagemEstavel, _dadosGerais.TipoViolacao);
                }

                // Simular dados das plataformas
                for (int i = 0; i < _plataformas.Length; i++)
                {
                    var plataforma = _plataformas[i];
                    if (plataforma == null)
                        continue;

                    var dadosSimulados = GerarDadosSimulados(i + 1);
                    if (plataforma.ProcessarDadosRecebidos(dadosSimulados, i + 1))
                    {
                        // Atualizar dados na cole��o base
                        AtualizarDadosPlataforma(i + 1, (decimal)plataforma.Peso, (decimal)plataforma.PesoBruto,
                            plataforma.PesagemEstavel, plataforma.TipoViolacao,
                            plataforma.Conectada ? StatusPlataforma.Conectada : StatusPlataforma.Desconectada);
                    }
                }

                // Verificar notifica��o de carga viva
                VerificarNotificacaoCargaViva();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na simula��o: {ex.Message}");
            }
        }

        /// <summary>
        /// Gera dados simulados para uma plataforma.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma (0 para dados gerais, 1-N para plataformas espec�ficas).</param>
        /// <returns>Array de bytes simulados.</returns>
        private byte[] GerarDadosSimulados(int idPlataforma)
        {
            var dados = new byte[20];

            // Gerar peso simulado
            float peso = 0;
            if (idPlataforma == 0 && _dadosGerais != null)
            {
                // Para dados gerais, somar peso de todas as plataformas
                peso = _plataformas.Where(p => p != null).Sum(p => p.Peso);
            }
            else
            {
                // Para plataforma individual, gerar peso aleat�rio
                peso = 100 + (float)(_geradorAleatorio.NextDouble() * 900);
                if (_geradorAleatorio.Next(100) < 5) // 5% de chance de ser zero
                    peso = 0;
            }

            // Preencher dados b�sicos
            BitConverter.GetBytes(peso).CopyTo(dados, 0); // Peso (bytes 0-3)
            BitConverter.GetBytes(peso * 1.1f).CopyTo(dados, 4); // Peso bruto (bytes 4-7)

            // Byte 8: Quantidade de plataformas (para dados gerais) ou tipo de conex�o
            dados[8] = (byte)(idPlataforma == 0 ? (_dadosGerais?.QuantidadePlataformas ?? 4) : 0);

            // Byte 9: Casas decimais
            dados[9] = 2;

            // Bytes 10-11: Status
            GerarStatusSimulado(dados, peso);

            // Bytes 12-13: Bateria
            var tensaoBateria = (short)_geradorAleatorio.Next(5700, 8400);
            BitConverter.GetBytes(tensaoBateria).CopyTo(dados, 12);

            return dados;
        }

        /// <summary>
        /// Gera status simulado para os dados.
        /// </summary>
        /// <param name="dados">Array de dados para preencher.</param>
        /// <param name="peso">Peso atual para c�lculos.</param>
        private void GerarStatusSimulado(byte[] dados, float peso)
        {
            byte status1 = 0;
            byte status2 = 0;

            // Status 1
            status1 |= 1; // Bit 0: Conectado
            status1 |= (1 << 1); // Bit 1: Pesagem aferida

            if (_geradorAleatorio.Next(100) < 50) // 50% de chance de estar tarado
                status1 |= (1 << 2);

            if (_geradorAleatorio.Next(100) < 90) // 90% de chance de estar est�vel
                status1 |= (1 << 5);

            if (peso < 0.1) // Zero se peso baixo
                status1 |= (1 << 6);

            var cargaVivaHabilitada = _geradorAleatorio.Next(100) < 30; // 30% de chance
            if (cargaVivaHabilitada)
                status1 |= (1 << 7);

            // Status 2
            if (cargaVivaHabilitada && _geradorAleatorio.Next(100) < 70) // 70% de chance de fixar se habilitada
                status2 |= 1;

            if (_geradorAleatorio.Next(100) < 20) // 20% de chance de estar carregando
                status2 |= (1 << 1);

            dados[10] = status1;
            dados[11] = status2;
        }

        /// <summary>
        /// Verifica se deve notificar sobre carga viva.
        /// </summary>
        private void VerificarNotificacaoCargaViva()
        {
            if (!_jaTeveAlgumaPesagemNovaAposConectar)
            {
                if (TodosPlataformasLidas() && !TodosCargaVivaFixou())
                {
                    _jaTeveAlgumaPesagemNovaAposConectar = true;
                }
                return;
            }

            var antigoCargaVivaFixou = _dadosGerais?.CargaVivaFixou ?? false;
            if (TodosCargaVivaFixou() && !antigoCargaVivaFixou)
            {
                if (_dadosGerais != null)
                    _dadosGerais.CargaVivaFixou = true;

                NotificarCargaVivaFixada();
            }
            else if (_dadosGerais != null)
            {
                _dadosGerais.CargaVivaFixou = TodosCargaVivaFixou();
            }
        }

        /// <summary>
        /// Verifica se todas as plataformas foram lidas.
        /// </summary>
        private bool TodosPlataformasLidas()
        {
            return _plataformas.All(p => p != null && p.PesoDefinido);
        }

        /// <summary>
        /// Verifica se todas as plataformas t�m carga viva fixada.
        /// </summary>
        private bool TodosCargaVivaFixou()
        {
            return _plataformas.All(p => p != null && p.CargaVivaFixou);
        }

        /// <summary>
        /// Notifica que a carga viva foi fixada.
        /// </summary>
        private void NotificarCargaVivaFixada()
        {
            try
            {
                // Aqui pode implementar vibra��o, som ou outra notifica��o
                System.Diagnostics.Debug.WriteLine("Carga Viva Fixou - Notifica��o do Dispositivo Virtual");

                // Disparar evento personalizado se necess�rio
                // CargaVivaFixada?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na notifica��o de carga viva: {ex.Message}");
            }
        }
        #endregion

        #region Sobrescrita de M�todos
        /// <summary>
        /// Obt�m informa��es detalhadas do dispositivo virtual.
        /// </summary>
        public override Dictionary<string, object> ObterInformacoesDispositivo()
        {
            var info = base.ObterInformacoesDispositivo();

            info["TipoDispositivo"] = "Virtual";
            info["IntervaloSimulacao"] = IntervaloSimulacao;
            info["SimulacaoAtiva"] = SimulacaoAtiva;
            info["JaLeuDadosGeral"] = _jaLeuDadosGeral;
            info["PlataformasConfiguradas"] = _plataformas.Length;

            return info;
        }

        /// <summary>
        /// Libera recursos espec�ficos do dispositivo virtual.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerSimulacao?.Stop();
                _timerSimulacao?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}