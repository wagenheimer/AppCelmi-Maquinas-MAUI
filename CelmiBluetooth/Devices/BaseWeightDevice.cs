using CelmiBluetooth.Models;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;

namespace CelmiBluetooth.Devices
{
    /// <summary>
    /// Classe base abstrata para dispositivos de pesagem.
    /// Fornece funcionalidade comum para gerenciamento de peso e plataformas.
    /// </summary>
    public abstract partial class BaseDispositivoPesagem : ObservableObject, IDispositivoPesagem
    {
        #region Campos Privados
        private readonly Dictionary<string, object> _parametrosConexao = new();
        private CancellationTokenSource? _tokenCancelamentoLeitura;
        private bool _disposed = false;
        #endregion

        #region Propriedades Abstratas
        /// <summary>
        /// Nome do dispositivo.
        /// </summary>
        public abstract string Nome { get; }

        /// <summary>
        /// Status da conexão do dispositivo.
        /// </summary>
        public abstract string Status { get; }

        /// <summary>
        /// Tipo do dispositivo.
        /// </summary>
        public abstract TipoDispositivo Tipo { get; }

        /// <summary>
        /// Número total de plataformas suportadas.
        /// </summary>
        public abstract int TotalPlataformas { get; }

        /// <summary>
        /// Peso total de todas as plataformas ativas.
        /// </summary>
        public abstract decimal PesoTotal { get; }
        #endregion

        #region Propriedades Públicas
        /// <summary>
        /// Indica se o dispositivo está conectado.
        /// </summary>
        [ObservableProperty]
        protected bool _conectado;

        /// <summary>
        /// Lista de dados de peso das plataformas.
        /// </summary>
        public ObservableCollection<DadosPlataforma> DadosPlataformas { get; } = new();

        /// <summary>
        /// Intervalo atual de leitura em milissegundos.
        /// </summary>
        [ObservableProperty]
        protected int _intervaloLeitura = 250;

        /// <summary>
        /// Indica se a leitura está ativa.
        /// </summary>
        [ObservableProperty]
        protected bool _leituraAtiva;

        /// <summary>
        /// Última atualização de dados.
        /// </summary>
        [ObservableProperty]
        protected DateTime _ultimaAtualizacao;
        #endregion

        #region Eventos
        /// <summary>
        /// Evento disparado quando os dados de peso são atualizados.
        /// </summary>
        public event EventHandler<PesoAtualizadoEventArgs>? PesoAtualizado;

        /// <summary>
        /// Evento disparado quando o status de conexão muda.
        /// </summary>
        public event EventHandler<StatusConexaoEventArgs>? StatusConexaoAlterado;
        #endregion

        #region Métodos Abstratos
        /// <summary>
        /// Implementação específica de conexão do dispositivo.
        /// </summary>
        /// <param name="parametros">Parâmetros de conexão.</param>
        /// <returns>True se conectado com sucesso.</returns>
        protected abstract Task<bool> ConectarImplementacaoAsync(Dictionary<string, object>? parametros);

        /// <summary>
        /// Implementação específica de desconexão do dispositivo.
        /// </summary>
        /// <returns>Task representando a operação assíncrona.</returns>
        protected abstract Task DesconectarImplementacaoAsync();

        /// <summary>
        /// Implementação específica de tara de plataforma.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        protected abstract Task TararPlataformaImplementacaoAsync(int idPlataforma);

        /// <summary>
        /// Implementação específica de zero de plataforma.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        protected abstract Task ZerarPlataformaImplementacaoAsync(int idPlataforma);
        #endregion

        #region Métodos Públicos
        /// <summary>
        /// Conecta ao dispositivo.
        /// </summary>
        /// <param name="parametros">Parâmetros de conexão opcionais.</param>
        /// <returns>True se conectado com sucesso.</returns>
        public virtual async Task<bool> ConectarAsync(Dictionary<string, object>? parametros = null)
        {
            if (Conectado)
                return true;

            try
            {
                // Armazenar parâmetros para reconexão
                if (parametros != null)
                {
                    _parametrosConexao.Clear();
                    foreach (var param in parametros)
                    {
                        _parametrosConexao[param.Key] = param.Value;
                    }
                }

                var sucesso = await ConectarImplementacaoAsync(parametros);

                if (sucesso)
                {
                    Conectado = true;
                    InicializarPlataformas();
                    NotificarMudancaStatus(true);
                }
                else
                {
                    NotificarMudancaStatus(false, "Falha na conexão");
                }

                return sucesso;
            }
            catch (Exception ex)
            {
                NotificarMudancaStatus(false, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Conecta ao dispositivo sem parâmetros.
        /// </summary>
        /// <returns>True se conectado com sucesso.</returns>
        public virtual async Task<bool> ConectarAsync()
        {
            return await ConectarAsync(null);
        }

        /// <summary>
        /// Desconecta do dispositivo.
        /// </summary>
        public virtual async Task DesconectarAsync()
        {
            if (!Conectado)
                return;

            try
            {
                await PararLeituraValoresManuais();
                await DesconectarImplementacaoAsync();

                Conectado = false;
                LeituraAtiva = false;
                AtualizarStatusPlataformas(StatusPlataforma.Desconectada);
                NotificarMudancaStatus(false);
            }
            catch (Exception ex)
            {
                NotificarMudancaStatus(false, ex.Message);
            }
        }

        /// <summary>
        /// Inicia a leitura contínua dos pesos.
        /// </summary>
        /// <param name="intervalo">Intervalo entre leituras em milissegundos.</param>

        

        public virtual async Task IniciaLeituraValoresManuaisAsync(int intervalo = 250)
        {
            if (!Conectado || LeituraAtiva) return;

            IntervaloLeitura = intervalo;
            _tokenCancelamentoLeitura = new CancellationTokenSource();
            LeituraAtiva = true;

            // Executar leitura em tarefa em background
            _ = Task.Run(async () =>
            {
                try
                {
                    await CriaTaskLeituraValoresNoIntervalo(_tokenCancelamentoLeitura.Token);
                }
                catch (OperationCanceledException)
                {
                    // Operação cancelada - comportamento esperado
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro na leitura de peso: {ex.Message}");
                    LeituraAtiva = false;
                }
            }, _tokenCancelamentoLeitura.Token);
        }


        /// <summary>
        /// Implementação específica de leitura de peso contínua.
        /// </summary>
        public async Task CriaTaskLeituraValoresNoIntervalo(CancellationToken cancellationToken)
        {
            _tokenCancelamentoLeitura = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Inicia leitura periódica
            _ = Task.Run(async () => await IniciaLeituraValoresManuaisAsync(), _tokenCancelamentoLeitura.Token);

            try
            {
                // Manter a tarefa viva enquanto não for cancelada
                while (!cancellationToken.IsCancellationRequested && Conectado)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelamento esperado
            }
        }

        /// <summary>
        /// Para a leitura contínua dos pesos.
        /// </summary>
        public virtual async Task PararLeituraValoresManuais()
        {
            if (!LeituraAtiva)
                return;

            _tokenCancelamentoLeitura?.Cancel();
            _tokenCancelamentoLeitura?.Dispose();
            _tokenCancelamentoLeitura = null;
            LeituraAtiva = false;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Zera/tara uma plataforma específica.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma (1-N).</param>
        public virtual async Task TararPlataformaAsync(int idPlataforma)
        {
            if (!PodeExecutarComandos())
                return;

            await TararPlataformaImplementacaoAsync(idPlataforma);
        }

        /// <summary>
        /// Zera/tara todas as plataformas.
        /// </summary>
        public virtual async Task TararTodasPlataformasAsync()
        {
            if (!PodeExecutarComandos())
                return;

            var tarefas = DadosPlataformas.Select(p => TararPlataformaAsync(p.IdPlataforma));
            await Task.WhenAll(tarefas);
        }

        /// <summary>
        /// Realiza zero em uma plataforma específica.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma (1-N).</param>
        public virtual async Task ZerarPlataformaAsync(int idPlataforma)
        {
            if (!PodeExecutarComandos())
                return;

            await ZerarPlataformaImplementacaoAsync(idPlataforma);
        }

        /// <summary>
        /// Realiza zero em todas as plataformas.
        /// </summary>
        public virtual async Task ZerarTodasPlataformasAsync()
        {
            if (!PodeExecutarComandos())
                return;

            var tarefas = DadosPlataformas.Select(p => ZerarPlataformaAsync(p.IdPlataforma));
            await Task.WhenAll(tarefas);
        }

        /// <summary>
        /// Obtém o peso total formatado.
        /// </summary>
        public virtual string ObterPesoTotalFormatado()
        {
            if (!Conectado)
                return "Desconectado";

            // Usar 2 casas decimais por padrão, pois DadosPlataforma não possui CasasDecimais
            return $"{PesoTotal:F2} kg";
        }

        /// <summary>
        /// Obtém o peso formatado para uma plataforma específica.
        /// </summary>
        public virtual string ObterPesoPlataformaFormatado(int idPlataforma)
        {
            var plataforma = DadosPlataformas.FirstOrDefault(p => p.IdPlataforma == idPlataforma);
            return plataforma?.PesoFormatado ?? "Não encontrado";
        }

        /// <summary>
        /// Valida se o dispositivo pode executar comandos.
        /// </summary>
        public virtual bool PodeExecutarComandos()
        {
            return Conectado && !_disposed;
        }

        /// <summary>
        /// Obtém informações detalhadas do dispositivo.
        /// </summary>
        public virtual Dictionary<string, object> ObterInformacoesDispositivo()
        {
            return new Dictionary<string, object>
            {
                ["Nome"] = Nome,
                ["Tipo"] = Tipo.ToString(),
                ["Conectado"] = Conectado,
                ["TotalPlataformas"] = TotalPlataformas,
                ["PesoTotal"] = PesoTotal,
                ["LeituraAtiva"] = LeituraAtiva,
                ["IntervaloLeitura"] = IntervaloLeitura,
                ["UltimaAtualizacao"] = UltimaAtualizacao
            };
        }
        #endregion

        #region Métodos Protegidos
        /// <summary>
        /// Inicializa as plataformas disponíveis no dispositivo.
        /// </summary>
        protected virtual void InicializarPlataformas()
        {
            DadosPlataformas.Clear();

            for (int i = 1; i <= TotalPlataformas; i++)
            {
                DadosPlataformas.Add(new DadosPlataforma
                {
                    IdPlataforma = i,
                    Descricao = $"Plataforma {i}",
                    Status = StatusPlataforma.Desconectada
                });
            }
        }

        /// <summary>
        /// Atualiza o status de todas as plataformas.
        /// </summary>
        protected virtual void AtualizarStatusPlataformas(StatusPlataforma status)
        {
            foreach (var plataforma in DadosPlataformas)
            {
                plataforma.Status = status;
            }
        }

        /// <summary>
        /// Notifica mudança no status de conexão.
        /// </summary>
        protected virtual void NotificarMudancaStatus(bool conectado, string? mensagemErro = null, string? fase = null)
        {
            StatusConexaoAlterado?.Invoke(this, new StatusConexaoEventArgs
            {
                Conectado = conectado,
                MensagemErro = mensagemErro,
                TimestampMudanca = DateTime.Now
            });
        }

        /// <summary>
        /// Notifica atualização de peso.
        /// </summary>
        protected virtual void NotificarPesoAtualizado(int idPlataforma, decimal peso, bool estavel, TipoViolacaoPeso tipoViolacao = TipoViolacaoPeso.Normal)
        {
            UltimaAtualizacao = DateTime.Now;

            PesoAtualizado?.Invoke(this, new PesoAtualizadoEventArgs
            {
                IdPlataforma = idPlataforma,
                Peso = peso,
                TimestampAtualizacao = UltimaAtualizacao,
                Estavel = estavel,
                TipoViolacao = tipoViolacao
            });
        }

        /// <summary>
        /// Atualiza dados de uma plataforma específica.
        /// </summary>
        protected virtual void AtualizarDadosPlataforma(int idPlataforma, decimal peso, decimal pesoBruto, bool estavel,
            TipoViolacaoPeso tipoViolacao = TipoViolacaoPeso.Normal, StatusPlataforma? status = null)
        {
            var plataforma = DadosPlataformas.FirstOrDefault(p => p.IdPlataforma == idPlataforma);
            if (plataforma == null)
                return;

            plataforma.PesoAtual = peso;
            plataforma.Estavel = estavel;

            if (status.HasValue)
                plataforma.Status = status.Value;

            NotificarPesoAtualizado(idPlataforma, peso, estavel, tipoViolacao);
        }
        #endregion

        #region Implementação IDisposable
        /// <summary>
        /// Libera recursos do dispositivo.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera recursos do dispositivo.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    PararLeituraValoresManuais().Wait(1000);
                    DesconectarAsync().Wait(1000);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro durante dispose: {ex.Message}");
                }
                finally
                {
                    _tokenCancelamentoLeitura?.Dispose();
                    _disposed = true;
                }
            }
        }

        #endregion
    }
}