using CelmiBluetooth.Models;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

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
        private IDisposable? _leituraSubscription;
        private bool _disposed = false;
        #endregion

        #region Propriedades Abstratas
        /// <summary>
        /// Nome do dispositivo.
        /// </summary>
        public abstract string Nome { get; }

        /// <summary>
        /// Status da conex�o do dispositivo.
        /// </summary>
        public abstract string Status { get; }

        /// <summary>
        /// Tipo do dispositivo.
        /// </summary>
        public abstract TipoDispositivo Tipo { get; }

        /// <summary>
        /// N�mero total de plataformas suportadas.
        /// </summary>
        public abstract int TotalPlataformas { get; }

        /// <summary>
        /// Peso total de todas as plataformas ativas.
        /// </summary>
        public abstract decimal PesoTotal { get; }
        #endregion

        #region Propriedades P�blicas
        /// <summary>
        /// Indica se o dispositivo est� conectado.
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
        /// Indica se a leitura est� ativa.
        /// </summary>
        [ObservableProperty]
        protected bool _leituraAtiva;

        /// <summary>
        /// �ltima atualiza��o de dados.
        /// </summary>
        [ObservableProperty]
        protected DateTime _ultimaAtualizacao;
        #endregion

        #region Eventos
        /// <summary>
        /// Evento disparado quando os dados de peso s�o atualizados.
        /// </summary>
        public event EventHandler<PesoAtualizadoEventArgs>? PesoAtualizado;

        /// <summary>
        /// Evento disparado quando o status de conex�o muda.
        /// </summary>
        public event EventHandler<StatusConexaoEventArgs>? StatusConexaoAlterado;
        #endregion

        #region M�todos Abstratos
        /// <summary>
        /// Implementa��o espec�fica de conex�o do dispositivo.
        /// </summary>
        /// <param name="parametros">Par�metros de conex�o.</param>
        /// <returns>True se conectado com sucesso.</returns>
        protected abstract Task<bool> ConectarImplementacaoAsync(Dictionary<string, object>? parametros);

        /// <summary>
        /// Implementa��o espec�fica de desconex�o do dispositivo.
        /// </summary>
        /// <returns>Task representando a opera��o ass�ncrona.</returns>
        protected abstract Task DesconectarImplementacaoAsync();

        /// <summary>
        /// Implementa��o espec�fica de tara de plataforma.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma.</param>
        /// <returns>Task representando a opera��o ass�ncrona.</returns>
        protected abstract Task TararPlataformaImplementacaoAsync(int idPlataforma);

        /// <summary>
        /// Implementa��o espec�fica de zero de plataforma.
        /// </summary>
        /// <param name="idPlataforma">ID da plataforma.</param>
        /// <returns>Task representando a opera��o ass�ncrona.</returns>
        protected abstract Task ZerarPlataformaImplementacaoAsync(int idPlataforma);
        #endregion

        #region M�todos P�blicos
        /// <summary>
        /// Conecta ao dispositivo.
        /// </summary>
        /// <param name="parametros">Par�metros de conex�o opcionais.</param>
        /// <returns>True se conectado com sucesso.</returns>
        public virtual async Task<bool> ConectarAsync(Dictionary<string, object>? parametros = null)
        {
            if (Conectado)
                return true;

            try
            {
                // Armazenar par�metros para reconex�o
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
                    NotificarMudancaStatus(false, "Falha na conex�o");
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
        /// Conecta ao dispositivo sem par�metros.
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
        /// Inicia a leitura cont�nua dos pesos.
        /// </summary>
        /// <param name="intervalo">Intervalo entre leituras em milissegundos.</param>

        /// <summary>
        /// Inicia a leitura cont�nua dos valores no intervalo definido usando System.Reactive.
        /// </summary>
        /// <returns>Task que representa a opera��o ass�ncrona.</returns>
        public virtual Task IniciaLeituraValoresManuaisAsync()
        {
            if (!Conectado || LeituraAtiva)
                return Task.CompletedTask;

            try
            {
                _tokenCancelamentoLeitura = new CancellationTokenSource();
                LeituraAtiva = true;

                // Criar Observable que emite no intervalo especificado com timeout
                _leituraSubscription = Observable
                    .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(IntervaloLeitura))
                    .TakeWhile(_ => Conectado && !_tokenCancelamentoLeitura.Token.IsCancellationRequested)
                    .Timeout(TimeSpan.FromSeconds(30)) // Timeout para detectar travamentos
                    .Subscribe(
                        async _ =>
                        {
                            try
                            {
                                if (!Conectado || _tokenCancelamentoLeitura.Token.IsCancellationRequested)
                                    return;

                                // Chamada para o m�todo virtual que deve ser implementado pelas classes derivadas
                                await LerValoresManuaisNoIntervalo().ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                // Cancelamento esperado - n�o logar como erro
                                System.Diagnostics.Debug.WriteLine("Leitura de peso cancelada");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Erro na leitura de peso: {ex.Message}");
                                // N�o para a leitura por causa de um erro pontual
                            }
                        },
                        error =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro fatal na leitura de peso: {error.Message}");
                            LeituraAtiva = false;
                        },
                        () =>
                        {
                            System.Diagnostics.Debug.WriteLine("Leitura de peso finalizada");
                            LeituraAtiva = false;
                        }
                    );

                System.Diagnostics.Debug.WriteLine($"Leitura de peso iniciada com intervalo de {IntervaloLeitura}ms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao iniciar leitura de peso: {ex.Message}");
                LeituraAtiva = false;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Para a leitura cont�nua dos pesos.
        /// </summary>
        public virtual async Task PararLeituraValoresManuais()
        {
            if (!LeituraAtiva)
                return;

            try
            {
                // Cancelar o token primeiro
                _tokenCancelamentoLeitura?.Cancel();
                
                // Dispose da subscription do Observable
                _leituraSubscription?.Dispose();
                _leituraSubscription = null;
                
                // Cleanup dos recursos
                _tokenCancelamentoLeitura?.Dispose();
                _tokenCancelamentoLeitura = null;
                
                LeituraAtiva = false;
                
                System.Diagnostics.Debug.WriteLine("Leitura de peso parada com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao parar leitura de peso: {ex.Message}");
                LeituraAtiva = false;
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Zera/tara uma plataforma espec�fica.
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
        /// Realiza zero em uma plataforma espec�fica.
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
        /// Obt�m o peso total formatado.
        /// </summary>
        public virtual string ObterPesoTotalFormatado()
        {
            if (!Conectado)
                return "Desconectado";

            // Usar 2 casas decimais por padr�o, pois DadosPlataforma n�o possui CasasDecimais
            return $"{PesoTotal:F2} kg";
        }

        /// <summary>
        /// Obt�m o peso formatado para uma plataforma espec�fica.
        /// </summary>
        public virtual string ObterPesoPlataformaFormatado(int idPlataforma)
        {
            var plataforma = DadosPlataformas.FirstOrDefault(p => p.IdPlataforma == idPlataforma);
            return plataforma?.PesoFormatado ?? "N�o encontrado";
        }

        /// <summary>
        /// Valida se o dispositivo pode executar comandos.
        /// </summary>
        public virtual bool PodeExecutarComandos()
        {
            return Conectado && !_disposed;
        }

        /// <summary>
        /// Obt�m informa��es detalhadas do dispositivo.
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

        #region M�todos Protegidos
        /// <summary>
        /// Inicializa as plataformas dispon�veis no dispositivo.
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
        /// Notifica mudan�a no status de conex�o.
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
        /// Notifica atualiza��o de peso.
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
        /// Atualiza dados de uma plataforma espec�fica.
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

        #region Implementa��o IDisposable
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
                    // Usar ConfigureAwait(false) e timeout mais adequado
                    var stopTask = PararLeituraValoresManuais();
                    if (!stopTask.Wait(TimeSpan.FromSeconds(5)))
                    {
                        System.Diagnostics.Debug.WriteLine("Timeout ao parar leitura durante dispose");
                    }

                    var disconnectTask = DesconectarAsync();
                    if (!disconnectTask.Wait(TimeSpan.FromSeconds(5)))
                    {
                        System.Diagnostics.Debug.WriteLine("Timeout ao desconectar durante dispose");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro durante dispose: {ex.Message}");
                }
                finally
                {
                    // Cleanup garantido de todos os recursos IDisposable
                    try
                    {
                        _leituraSubscription?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao fazer dispose da leituraSubscription: {ex.Message}");
                    }
                    finally
                    {
                        _leituraSubscription = null;
                    }

                    try
                    {
                        _tokenCancelamentoLeitura?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao fazer dispose do tokenCancelamentoLeitura: {ex.Message}");
                    }
                    finally
                    {
                        _tokenCancelamentoLeitura = null;
                    }

                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// L� os valores iniciais do dispositivo.
        /// </summary>
        /// <returns></returns>
        public virtual Task LerValoresIniciaisAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// L� os valores manuais no intervalo definido.
        /// </summary>
        /// <returns></returns>

        public Task LerValoresManuaisNoIntervalo()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Novos M�todos

        /// <summary>
        /// Altera o intervalo de leitura dinamicamente.
        /// Se a leitura estiver ativa, reinicia com o novo intervalo.
        /// </summary>
        /// <param name="novoIntervalo">Novo intervalo em milissegundos (m�nimo 100ms).</param>
        /// <param name="cancellationToken">Token de cancelamento para a opera��o.</param>
        public virtual async Task AlterarIntervaloLeituraAsync(int novoIntervalo, CancellationToken cancellationToken = default)
        {
            if (novoIntervalo < 100)
                throw new ArgumentOutOfRangeException(nameof(novoIntervalo), "Intervalo m�nimo � 100ms");

            var estavaAtiva = LeituraAtiva;
            
            if (estavaAtiva)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(10)); // Timeout de 10 segundos
                
                await PararLeituraValoresManuais().ConfigureAwait(false);
            }
            
            IntervaloLeitura = novoIntervalo;
            
            if (estavaAtiva && Conectado && !cancellationToken.IsCancellationRequested)
            {
                await IniciaLeituraValoresManuaisAsync().ConfigureAwait(false);
            }
            
            System.Diagnostics.Debug.WriteLine($"Intervalo de leitura alterado para {novoIntervalo}ms");
        }

        #endregion
    }
}