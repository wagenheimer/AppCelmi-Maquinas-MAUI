using CelmiBluetooth.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth.Platforms
{
    /// <summary>
    /// Protocolo de comunicação da plataforma.
    /// </summary>
    public enum ProtocoloPlataforma
    {
        /// <summary>
        /// Protocolo RX (Receptor/Visor).
        /// </summary>
        RX,
        
        /// <summary>
        /// Protocolo TX (Transmissor/Plataforma Individual).
        /// </summary>
        TX
    }

    /// <summary>
    /// Tipo de conexão da plataforma.
    /// </summary>
    public enum TipoConexaoPlataforma
    {
        /// <summary>
        /// Conexão Bluetooth.
        /// </summary>
        Bluetooth = 0,
        
        /// <summary>
        /// Conexão por cabo.
        /// </summary>
        Cabo = 1,
        
        /// <summary>
        /// Conexão WiFi.
        /// </summary>
        WiFi = 2
    }

    /// <summary>
    /// Classe base para representar dados de uma plataforma de pesagem.
    /// Unifica a funcionalidade das plataformas RX e TX.
    /// </summary>
    public abstract partial class PlataformaBase : ObservableObject
    {
        #region Propriedades Básicas
        /// <summary>
        /// Nome identificador da plataforma.
        /// </summary>
        [ObservableProperty]
        protected string _nome = string.Empty;

        /// <summary>
        /// Peso atual (líquido) em kg.
        /// </summary>
        [ObservableProperty]
        protected float _peso;

        /// <summary>
        /// Peso bruto em kg.
        /// </summary>
        [ObservableProperty]
        protected float _pesoBruto;

        /// <summary>
        /// Peso da tara em kg.
        /// </summary>
        [ObservableProperty]
        protected float _tara;

        /// <summary>
        /// Número de casas decimais para exibição.
        /// </summary>
        [ObservableProperty]
        protected int _casasDecimais = 2;
        #endregion

        #region Estado da Plataforma
        /// <summary>
        /// Indica se a plataforma está conectada.
        /// </summary>
        [ObservableProperty]
        protected bool _conectada;

        /// <summary>
        /// Indica se a pesagem foi aferida/calibrada.
        /// </summary>
        [ObservableProperty]
        protected bool _pesagemAferida;

        /// <summary>
        /// Indica se a plataforma está tarada.
        /// </summary>
        [ObservableProperty]
        protected bool _tarado;

        /// <summary>
        /// Indica se há sobrecarga.
        /// </summary>
        [ObservableProperty]
        protected bool _sobrecarga;

        /// <summary>
        /// Indica se há subcarga.
        /// </summary>
        [ObservableProperty]
        protected bool _subcarga;

        /// <summary>
        /// Indica se o peso está em zero.
        /// </summary>
        [ObservableProperty]
        protected bool _zero;

        /// <summary>
        /// Indica se a pesagem está estável.
        /// </summary>
        [ObservableProperty]
        protected bool _pesagemEstavel;

        /// <summary>
        /// Tipo de conexão da plataforma.
        /// </summary>
        [ObservableProperty]
        protected TipoConexaoPlataforma _tipoConexao = TipoConexaoPlataforma.Bluetooth;
        #endregion

        #region Carga Viva
        /// <summary>
        /// Indica se a carga viva está habilitada.
        /// </summary>
        [ObservableProperty]
        protected bool _cargaVivaHabilitada;

        /// <summary>
        /// Indica se a carga viva foi fixada.
        /// </summary>
        [ObservableProperty]
        protected bool _cargaVivaFixou;
        #endregion

        #region Bateria
        /// <summary>
        /// Indica se a bateria está carregando.
        /// </summary>
        [ObservableProperty]
        protected bool _carregandoBateria;

        /// <summary>
        /// Porcentagem da bateria (0-100).
        /// </summary>
        [ObservableProperty]
        protected int _porcentagemBateria;
        #endregion

        #region Identificação e Rede
        /// <summary>
        /// Número da rede (para dispositivos TX).
        /// </summary>
        [ObservableProperty]
        protected int _numeroRede;

        /// <summary>
        /// Número da plataforma na rede.
        /// </summary>
        [ObservableProperty]
        protected int _numeroPlataforma;

        /// <summary>
        /// Quantidade total de plataformas (para dispositivos RX).
        /// </summary>
        [ObservableProperty]
        protected int _quantidadePlataformas;

        /// <summary>
        /// Versão do firmware.
        /// </summary>
        [ObservableProperty]
        protected int _versaoFirmware;
        #endregion

        #region Controle de Estado
        /// <summary>
        /// Timestamp da última leitura.
        /// </summary>
        [ObservableProperty]
        protected DateTime _ultimaLeitura = DateTime.Now;

        /// <summary>
        /// Indica se o peso já foi definido pelo menos uma vez.
        /// </summary>
        [ObservableProperty]
        protected bool _pesoDefinido;
        #endregion

        #region Propriedades Calculadas
        /// <summary>
        /// Indica se a plataforma precisa piscar (modo carga viva ativo mas não fixado).
        /// </summary>
        public virtual bool PrecisaPiscar => CargaVivaHabilitada && !CargaVivaFixou && !PesagemEstavel;

        /// <summary>
        /// Retorna informações sobre a bateria.
        /// </summary>
        public virtual string InfoBateria => PorcentagemBateria > 0 ? 
            $"{PorcentagemBateria}%" + (CarregandoBateria ? " (Carregando)" : "") : 
            string.Empty;

        /// <summary>
        /// Obtém o tipo de violação atual.
        /// </summary>
        public virtual TipoViolacaoPeso TipoViolacao
        {
            get
            {
                if (Sobrecarga) return TipoViolacaoPeso.Sobrecarga;
                if (Subcarga) return TipoViolacaoPeso.Subcarga;
                return TipoViolacaoPeso.Normal;
            }
        }

        /// <summary>
        /// Verifica se o dispositivo possui funcionalidade de peso bruto.
        /// </summary>
        public virtual bool PossuiPesoBruto => VersaoFirmware >= 35;

        /// <summary>
        /// Protocolo de comunicação da plataforma.
        /// </summary>
        public abstract ProtocoloPlataforma Protocolo { get; }
        #endregion

        #region Métodos de Processamento de Dados
        /// <summary>
        /// Desmembra os valores recebidos pelos bytes do dispositivo.
        /// </summary>
        /// <param name="dados">Bytes com informações de pesagem.</param>
        /// <param name="identificador">Identificador específico do protocolo (plataformaId para RX, característica para TX).</param>
        /// <returns>True se houve alteração nos valores.</returns>
        public abstract bool ProcessarDadosRecebidos(byte[] dados, object identificador);

        /// <summary>
        /// Aplica um comando de tara na plataforma.
        /// </summary>
        public virtual void AplicarTara()
        {
            if (!Tarado)
            {
                Tarado = true;
                Tara = PesoBruto - Peso;
            }
            else
            {
                Tarado = false;
                Tara = 0;
            }
        }

        /// <summary>
        /// Aplica um comando de zero na plataforma.
        /// </summary>
        public virtual void AplicarZero()
        {
            Peso = 0;
            PesoBruto = 0;
            Tara = 0;
            Tarado = false;
            Zero = true;
        }

        /// <summary>
        /// Reseta as variáveis para os valores iniciais.
        /// </summary>
        public virtual void ResetarValoresPadrao()
        {
            Peso = 0;
            PesoBruto = 0;
            Tara = 0;
            PesagemAferida = false;
            Sobrecarga = false;
            Subcarga = false;
            CasasDecimais = 2;
            VersaoFirmware = 0;
            Tarado = false;
            PesoDefinido = false;
            CargaVivaFixou = false;
            PesagemEstavel = false;
            PorcentagemBateria = 0;
            CarregandoBateria = false;
            UltimaLeitura = DateTime.Now;
        }
        #endregion

        #region Métodos de Formatação
        /// <summary>
        /// Obtém o peso formatado para exibição.
        /// </summary>
        /// <param name="usarPesoBruto">Se deve usar o peso bruto em vez do peso líquido.</param>
        /// <returns>Peso formatado como string.</returns>
        public virtual string ObterPesoFormatado(bool usarPesoBruto = false)
        {
            if (!Conectada)
                return "Desconectado";

            if (!PesagemAferida)
                return "Não Aferido";

            var tipoViolacao = TipoViolacao;
            if (tipoViolacao == TipoViolacaoPeso.Sobrecarga)
                return "Sobrecarga";
            
            if (tipoViolacao == TipoViolacaoPeso.Subcarga)
                return "Subcarga";

            // Se for carga viva, só mostra se estiver fixado
            if (CargaVivaHabilitada && !CargaVivaFixou && !PesagemEstavel)
                return "- - - - - -";

            var pesoParaExibir = usarPesoBruto ? PesoBruto : Peso;
            var formato = $"F{CasasDecimais}";
            return $"{pesoParaExibir.ToString(formato)} kg";
        }

        /// <summary>
        /// Obtém uma representação string das informações da plataforma.
        /// </summary>
        /// <returns>String com informações resumidas.</returns>
        public virtual string ObterInformacoes()
        {
            return $"{Nome} - {ObterPesoFormatado()} - {(Conectada ? "Conectado" : "Desconectado")}";
        }
        #endregion

        #region Métodos Utilitários Protegidos
        /// <summary>
        /// Calcula a porcentagem da bateria baseada na tensão.
        /// </summary>
        /// <param name="tensaoEmMilivolts">Tensão da bateria em milivolts.</param>
        /// <returns>Porcentagem da bateria (0-100).</returns>
        protected virtual int CalcularPorcentagemBateria(int tensaoEmMilivolts)
        {
            // Valores típicos para bateria de lítio
            const int tensaoMinima = 5700; // 5.7V em mV
            const int tensaoMaxima = 8400; // 8.4V em mV

            if (tensaoEmMilivolts < tensaoMinima)
                return 0;
            if (tensaoEmMilivolts > tensaoMaxima)
                return 100;

            return (tensaoEmMilivolts - tensaoMinima) * 100 / (tensaoMaxima - tensaoMinima);
        }

        /// <summary>
        /// Verifica se houve alteração significativa no peso.
        /// </summary>
        /// <param name="novoPeso">Novo valor do peso.</param>
        /// <param name="tolerancia">Tolerância para considerar alteração.</param>
        /// <returns>True se houve alteração significativa.</returns>
        protected virtual bool HouveAlteracaoPeso(float novoPeso, float tolerancia = 0.001f)
        {
            return Math.Abs(Peso - novoPeso) > tolerancia;
        }

        /// <summary>
        /// Atualiza propriedades básicas se houver alteração.
        /// </summary>
        /// <param name="novoPeso">Novo peso.</param>
        /// <param name="novoPesoBruto">Novo peso bruto.</param>
        /// <returns>True se houve alguma alteração.</returns>
        protected virtual bool AtualizarPesosSeNecessario(float novoPeso, float novoPesoBruto)
        {
            var modificou = false;

            if (HouveAlteracaoPeso(novoPeso))
            {
                Peso = novoPeso;
                PesoDefinido = true;
                modificou = true;
            }

            if (Math.Abs(PesoBruto - novoPesoBruto) > 0.001f)
            {
                PesoBruto = novoPesoBruto;
                modificou = true;
            }

            if (modificou)
            {
                UltimaLeitura = DateTime.Now;
                
                // Recalcula a tara se estiver tarado
                if (Tarado)
                {
                    Tara = PesoBruto - Peso;
                }
            }

            return modificou;
        }
        #endregion
    }
}