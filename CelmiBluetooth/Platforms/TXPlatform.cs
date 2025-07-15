using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth.Platforms
{
    /// <summary>
    /// Representa uma plataforma de pesagem de um dispositivo TX (Transmissor).
    /// Implementa o protocolo de comunicação específico do TX.
    /// </summary>
    public partial class PlataformaTX : PlataformaBase
    {
        #region Propriedades do Protocolo TX
        /// <summary>
        /// Protocolo de comunicação (sempre TX).
        /// </summary>
        public override ProtocoloPlataforma Protocolo => ProtocoloPlataforma.TX;
        #endregion

        #region Construtores
        /// <summary>
        /// Inicializa uma nova instância da plataforma TX.
        /// </summary>
        public PlataformaTX()
        {
            Nome = "Plataforma TX";
            ResetarValoresPadrao();
        }

        /// <summary>
        /// Inicializa uma nova instância da plataforma TX com nome específico.
        /// </summary>
        /// <param name="nome">Nome da plataforma.</param>
        public PlataformaTX(string nome) : this()
        {
            Nome = nome;
        }

        /// <summary>
        /// Inicializa uma nova instância da plataforma TX com configuração específica.
        /// </summary>
        /// <param name="numeroRede">Número da rede.</param>
        /// <param name="numeroPlataforma">Número da plataforma na rede.</param>
        public PlataformaTX(int numeroRede, int numeroPlataforma) : this()
        {
            NumeroRede = numeroRede;
            NumeroPlataforma = numeroPlataforma;
            Nome = $"TX R{numeroRede}P{numeroPlataforma}";
        }
        #endregion

        #region Implementação do Processamento de Dados
        /// <summary>
        /// Desmembra os valores recebidos pelos bytes do dispositivo TX.
        /// </summary>
        /// <param name="dados">Bytes com informações de pesagem.</param>
        /// <param name="identificador">UUID da característica BLE.</param>
        /// <returns>True se houve alteração nos valores.</returns>
        public override bool ProcessarDadosRecebidos(byte[] dados, object identificador)
        {
            if (dados == null || dados.Length == 0 || identificador is not string caracteristica)
                return false;

            var modificou = false;

            try
            {
                modificou = caracteristica switch
                {
                    "PESO" => ProcessarPeso(dados),
                    "PESO_BRUTO" => ProcessarPesoBruto(dados),
                    "AFERIDO" => ProcessarAferido(dados),
                    "SOBRECARGA" => ProcessarSobrecarga(dados),
                    "SOBCARGA" => ProcessarSubcarga(dados),
                    "CASAS_DECIMAIS" => ProcessarCasasDecimais(dados),
                    "VERSAO_FIRMWARE" => ProcessarVersaoFirmware(dados),
                    "TARADO" => ProcessarTarado(dados),
                    "ESTAVEL" => ProcessarEstavel(dados),
                    "NUMERO_REDE" => ProcessarNumeroRede(dados),
                    "NUMERO_PLATAFORMA" => ProcessarNumeroPlataforma(dados),
                    "TENSAO_BATERIA" => ProcessarTensaoBateria(dados),
                    "CARREGANDO_BATERIA" => ProcessarCarregandoBateria(dados),
                    "CV_HAB" => ProcessarCargaVivaHabilitada(dados),
                    "CV_FIXOU" => ProcessarCargaVivaFixou(dados),
                    _ => false
                };

                if (modificou)
                {
                    UltimaLeitura = DateTime.Now;
                }

                return modificou;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar dados TX para {caracteristica}: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Métodos de Processamento de Características
        /// <summary>
        /// Processa dados de peso.
        /// </summary>
        private bool ProcessarPeso(byte[] dados)
        {
            if (dados.Length < 4) return false;

            float novoPeso;
            if (VersaoFirmware > 50)
            {
                // Firmware recente: peso como float
                novoPeso = BitConverter.ToSingle(dados, 0);
            }
            else
            {
                // Firmware antigo: peso em gramas como inteiro
                var pesoGramas = BitConverter.ToInt32(dados, 0);
                novoPeso = pesoGramas / 1000f; // Conversão de gramas para kg
            }

            if (HouveAlteracaoPeso(novoPeso))
            {
                Peso = novoPeso;
                PesoDefinido = true;

                // Recalcula a tara se estiver tarado
                if (Tarado)
                {
                    Tara = PesoBruto - Peso;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa dados de peso bruto.
        /// </summary>
        private bool ProcessarPesoBruto(byte[] dados)
        {
            if (dados.Length < 4) return false;

            var novoPesoBruto = BitConverter.ToSingle(dados, 0);
            if (Math.Abs(PesoBruto - novoPesoBruto) > 0.001f)
            {
                PesoBruto = novoPesoBruto;

                // Atualiza a tara se estiver tarado
                if (Tarado)
                {
                    Tara = PesoBruto - Peso;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de aferição.
        /// </summary>
        private bool ProcessarAferido(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoAferido = BitConverter.ToBoolean(dados, 0);
            if (PesagemAferida != novoAferido)
            {
                PesagemAferida = novoAferido;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de sobrecarga.
        /// </summary>
        private bool ProcessarSobrecarga(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoSobrecarga = BitConverter.ToBoolean(dados, 0);
            if (Sobrecarga != novoSobrecarga)
            {
                Sobrecarga = novoSobrecarga;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de subcarga.
        /// </summary>
        private bool ProcessarSubcarga(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoSubcarga = BitConverter.ToBoolean(dados, 0);
            if (Subcarga != novoSubcarga)
            {
                Subcarga = novoSubcarga;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa número de casas decimais.
        /// </summary>
        private bool ProcessarCasasDecimais(byte[] dados)
        {
            if (dados.Length < 4) return false;

            var novasCasas = BitConverter.ToInt32(dados, 0);
            if (CasasDecimais != novasCasas)
            {
                CasasDecimais = novasCasas;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa versão do firmware.
        /// </summary>
        private bool ProcessarVersaoFirmware(byte[] dados)
        {
            if (dados.Length < 4) return false;

            var novaVersao = BitConverter.ToInt32(dados, 0);
            if (VersaoFirmware != novaVersao)
            {
                VersaoFirmware = novaVersao;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de tara.
        /// </summary>
        private bool ProcessarTarado(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoTarado = BitConverter.ToBoolean(dados, 0);
            if (Tarado != novoTarado)
            {
                Tarado = novoTarado;

                // Se não está tarado, zera a tara
                if (!Tarado)
                {
                    Tara = 0;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de estabilidade.
        /// </summary>
        private bool ProcessarEstavel(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoEstavel = BitConverter.ToBoolean(dados, 0);
            if (PesagemEstavel != novoEstavel)
            {
                PesagemEstavel = novoEstavel;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa número da rede.
        /// </summary>
        private bool ProcessarNumeroRede(byte[] dados)
        {
            if (dados.Length < 4) return false;

            var novoNumeroRede = BitConverter.ToInt32(dados, 0);
            if (NumeroRede != novoNumeroRede)
            {
                NumeroRede = novoNumeroRede;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa número da plataforma.
        /// </summary>
        private bool ProcessarNumeroPlataforma(byte[] dados)
        {
            if (dados.Length < 4) return false;

            var novoNumeroPlataforma = BitConverter.ToInt32(dados, 0);
            if (NumeroPlataforma != novoNumeroPlataforma)
            {
                NumeroPlataforma = novoNumeroPlataforma;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa tensão da bateria.
        /// </summary>
        private bool ProcessarTensaoBateria(byte[] dados)
        {
            if (dados.Length < 4) return false;

            var tensaoBateria = BitConverter.ToInt32(dados, 0);
            var novoPercentual = CalcularPorcentagemBateria(tensaoBateria);
            if (PorcentagemBateria != novoPercentual)
            {
                PorcentagemBateria = novoPercentual;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de carregamento da bateria.
        /// </summary>
        private bool ProcessarCarregandoBateria(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoCarregandoBateria = BitConverter.ToBoolean(dados, 0);
            if (CarregandoBateria != novoCarregandoBateria)
            {
                CarregandoBateria = novoCarregandoBateria;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de carga viva habilitada.
        /// </summary>
        private bool ProcessarCargaVivaHabilitada(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoCargaVivaHabilitada = BitConverter.ToBoolean(dados, 0);
            if (CargaVivaHabilitada != novoCargaVivaHabilitada)
            {
                CargaVivaHabilitada = novoCargaVivaHabilitada;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processa status de carga viva fixada.
        /// </summary>
        private bool ProcessarCargaVivaFixou(byte[] dados)
        {
            if (dados.Length < 1) return false;

            var novoCargaVivaFixou = BitConverter.ToBoolean(dados, 0);
            if (CargaVivaFixou != novoCargaVivaFixou)
            {
                CargaVivaFixou = novoCargaVivaFixou;
                return true;
            }

            return false;
        }
        #endregion

        #region Métodos Específicos do TX
        /// <summary>
        /// Obtém o identificador único da plataforma TX.
        /// </summary>
        /// <returns>String identificadora no formato "R{rede}P{plataforma}".</returns>
        public string ObterIdentificador()
        {
            return $"R{NumeroRede}P{NumeroPlataforma}";
        }

        /// <summary>
        /// Verifica se a plataforma está pronta para pesagem.
        /// </summary>
        /// <returns>True se está pronta.</returns>
        public bool ProntaParaPesagem()
        {
            return Conectada && PesagemAferida && PesoDefinido && PesagemEstavel;
        }

        /// <summary>
        /// Obtém informações específicas do protocolo TX.
        /// </summary>
        /// <returns>Dicionário com informações específicas.</returns>
        public Dictionary<string, object> ObterInformacoesTX()
        {
            var info = new Dictionary<string, object>
            {
                ["Protocolo"] = "TX",
                ["NumeroRede"] = NumeroRede,
                ["NumeroPlataforma"] = NumeroPlataforma,
                ["Identificador"] = ObterIdentificador(),
                ["VersaoFirmware"] = VersaoFirmware,
                ["PossuiPesoBruto"] = PossuiPesoBruto,
                ["ProntaParaPesagem"] = ProntaParaPesagem()
            };

            if (PorcentagemBateria > 0)
            {
                info["Bateria"] = InfoBateria;
            }

            return info;
        }
        #endregion

        #region Sobrescrita de Métodos da Classe Base
        /// <summary>
        /// Reseta valores específicos do protocolo TX.
        /// </summary>
        public override void ResetarValoresPadrao()
        {
            base.ResetarValoresPadrao();
            NumeroRede = 0;
            NumeroPlataforma = 0;
            VersaoFirmware = 0;
        }

        /// <summary>
        /// Obtém informações específicas da plataforma TX.
        /// </summary>
        /// <returns>String com informações resumidas.</returns>
        public override string ObterInformacoes()
        {
            var info = base.ObterInformacoes();
            
            if (NumeroRede > 0 && NumeroPlataforma > 0)
            {
                info += $" - {ObterIdentificador()}";
            }

            if (VersaoFirmware > 0)
            {
                info += $" - FW v{VersaoFirmware}";
            }

            if (!string.IsNullOrEmpty(InfoBateria))
            {
                info += $" - {InfoBateria}";
            }

            return info;
        }
        #endregion
    }
}