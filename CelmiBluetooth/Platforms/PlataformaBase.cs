using CelmiBluetooth.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth.Platforms
{
    /// <summary>
    /// Protocolo de comunica��o da plataforma.
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
    /// Tipo de conex�o da plataforma.
    /// </summary>
    public enum TipoConexaoPlataforma
    {
        /// <summary>
        /// Conex�o Bluetooth.
        /// </summary>
        Bluetooth = 0,
        
        /// <summary>
        /// Conex�o por cabo.
        /// </summary>
        Cabo = 1,
        
        /// <summary>
        /// Conex�o WiFi.
        /// </summary>
        WiFi = 2
    }

    /// <summary>
    /// Classe base para representar dados de uma plataforma de pesagem.
    /// Unifica a funcionalidade das plataformas RX e TX.
    /// </summary>
    public abstract partial class PlataformaBase : ObservableObject
    {
        #region Propriedades B�sicas
        /// <summary>
        /// Nome identificador da plataforma.
        /// </summary>
        [ObservableProperty]
        protected string _nome = string.Empty;

        /// <summary>
        /// Peso atual (l�quido) em kg.
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
        /// N�mero de casas decimais para exibi��o.
        /// </summary>
        [ObservableProperty]
        protected int _casasDecimais = 2;
        #endregion

        #region Estado da Plataforma
        /// <summary>
        /// Indica se a plataforma est� conectada.
        /// </summary>
        [ObservableProperty]
        protected bool _conectada;

        /// <summary>
        /// Indica se a pesagem foi aferida/calibrada.
        /// </summary>
        [ObservableProperty]
        protected bool _pesagemAferida;

        /// <summary>
        /// Indica se a plataforma est� tarada.
        /// </summary>
        [ObservableProperty]
        protected bool _tarado;

        /// <summary>
        /// Indica se h� sobrecarga.
        /// </summary>
        [ObservableProperty]
        protected bool _sobrecarga;

        /// <summary>
        /// Indica se h� subcarga.
        /// </summary>
        [ObservableProperty]
        protected bool _subcarga;

        /// <summary>
        /// Indica se o peso est� em zero.
        /// </summary>
        [ObservableProperty]
        protected bool _zero;

        /// <summary>
        /// Indica se a pesagem est� est�vel.
        /// </summary>
        [ObservableProperty]
        protected bool _pesagemEstavel;

        /// <summary>
        /// Tipo de conex�o da plataforma.
        /// </summary>
        [ObservableProperty]
        protected TipoConexaoPlataforma _tipoConexao = TipoConexaoPlataforma.Bluetooth;
        #endregion

        #region Carga Viva
        /// <summary>
        /// Indica se a carga viva est� habilitada.
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
        /// Indica se a bateria est� carregando.
        /// </summary>
        [ObservableProperty]
        protected bool _carregandoBateria;

        /// <summary>
        /// Porcentagem da bateria (0-100).
        /// </summary>
        [ObservableProperty]
        protected int _porcentagemBateria;
        #endregion

        #region Identifica��o e Rede
        /// <summary>
        /// N�mero da rede (para dispositivos TX).
        /// </summary>
        [ObservableProperty]
        protected int _numeroRede;

        /// <summary>
        /// N�mero da plataforma na rede.
        /// </summary>
        [ObservableProperty]
        protected int _numeroPlataforma;

        /// <summary>
        /// Quantidade total de plataformas (para dispositivos RX).
        /// </summary>
        [ObservableProperty]
        protected int _quantidadePlataformas;

        /// <summary>
        /// Vers�o do firmware.
        /// </summary>
        [ObservableProperty]
        protected int _versaoFirmware;
        #endregion

        #region Controle de Estado
        /// <summary>
        /// Timestamp da �ltima leitura.
        /// </summary>
        [ObservableProperty]
        protected DateTime _ultimaLeitura = DateTime.Now;

        /// <summary>
        /// Indica se o peso j� foi definido pelo menos uma vez.
        /// </summary>
        [ObservableProperty]
        protected bool _pesoDefinido;
        #endregion

        #region Propriedades Calculadas
        /// <summary>
        /// Indica se a plataforma precisa piscar (modo carga viva ativo mas n�o fixado).
        /// </summary>
        public virtual bool PrecisaPiscar => CargaVivaHabilitada && !CargaVivaFixou && !PesagemEstavel;

        /// <summary>
        /// Retorna informa��es sobre a bateria.
        /// </summary>
        public virtual string InfoBateria => PorcentagemBateria > 0 ? 
            $"{PorcentagemBateria}%" + (CarregandoBateria ? " (Carregando)" : "") : 
            string.Empty;

        /// <summary>
        /// Obt�m o tipo de viola��o atual.
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
        /// Protocolo de comunica��o da plataforma.
        /// </summary>
        public abstract ProtocoloPlataforma Protocolo { get; }
        #endregion

        #region M�todos de Processamento de Dados
        /// <summary>
        /// Desmembra os valores recebidos pelos bytes do dispositivo.
        /// </summary>
        /// <param name="dados">Bytes com informa��es de pesagem.</param>
        /// <param name="identificador">Identificador espec�fico do protocolo (plataformaId para RX, caracter�stica para TX).</param>
        /// <returns>True se houve altera��o nos valores.</returns>
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
        /// Reseta as vari�veis para os valores iniciais.
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

        #region M�todos de Formata��o
        /// <summary>
        /// Obt�m o peso formatado para exibi��o.
        /// </summary>
        /// <param name="usarPesoBruto">Se deve usar o peso bruto em vez do peso l�quido.</param>
        /// <returns>Peso formatado como string.</returns>
        public virtual string ObterPesoFormatado(bool usarPesoBruto = false)
        {
            if (!Conectada)
                return "Desconectado";

            if (!PesagemAferida)
                return "N�o Aferido";

            var tipoViolacao = TipoViolacao;
            if (tipoViolacao == TipoViolacaoPeso.Sobrecarga)
                return "Sobrecarga";
            
            if (tipoViolacao == TipoViolacaoPeso.Subcarga)
                return "Subcarga";

            // Se for carga viva, s� mostra se estiver fixado
            if (CargaVivaHabilitada && !CargaVivaFixou && !PesagemEstavel)
                return "- - - - - -";

            var pesoParaExibir = usarPesoBruto ? PesoBruto : Peso;
            var formato = $"F{CasasDecimais}";
            return $"{pesoParaExibir.ToString(formato)} kg";
        }

        /// <summary>
        /// Obt�m uma representa��o string das informa��es da plataforma.
        /// </summary>
        /// <returns>String com informa��es resumidas.</returns>
        public virtual string ObterInformacoes()
        {
            return $"{Nome} - {ObterPesoFormatado()} - {(Conectada ? "Conectado" : "Desconectado")}";
        }
        #endregion

        #region M�todos Utilit�rios Protegidos
        /// <summary>
        /// Calcula a porcentagem da bateria baseada na tens�o.
        /// </summary>
        /// <param name="tensaoEmMilivolts">Tens�o da bateria em milivolts.</param>
        /// <returns>Porcentagem da bateria (0-100).</returns>
        protected virtual int CalcularPorcentagemBateria(int tensaoEmMilivolts)
        {
            // Valores t�picos para bateria de l�tio
            const int tensaoMinima = 5700; // 5.7V em mV
            const int tensaoMaxima = 8400; // 8.4V em mV

            if (tensaoEmMilivolts < tensaoMinima)
                return 0;
            if (tensaoEmMilivolts > tensaoMaxima)
                return 100;

            return (tensaoEmMilivolts - tensaoMinima) * 100 / (tensaoMaxima - tensaoMinima);
        }

        /// <summary>
        /// Verifica se houve altera��o significativa no peso.
        /// </summary>
        /// <param name="novoPeso">Novo valor do peso.</param>
        /// <param name="tolerancia">Toler�ncia para considerar altera��o.</param>
        /// <returns>True se houve altera��o significativa.</returns>
        protected virtual bool HouveAlteracaoPeso(float novoPeso, float tolerancia = 0.001f)
        {
            return Math.Abs(Peso - novoPeso) > tolerancia;
        }

        /// <summary>
        /// Atualiza propriedades b�sicas se houver altera��o.
        /// </summary>
        /// <param name="novoPeso">Novo peso.</param>
        /// <param name="novoPesoBruto">Novo peso bruto.</param>
        /// <returns>True se houve alguma altera��o.</returns>
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