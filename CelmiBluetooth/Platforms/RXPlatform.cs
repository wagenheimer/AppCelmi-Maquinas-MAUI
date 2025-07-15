using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth.Platforms
{
    /// <summary>
    /// Representa uma plataforma de pesagem de um dispositivo RX (Receptor/Visor).
    /// Implementa o protocolo de comunicação específico do RX.
    /// </summary>
    public partial class PlataformaRX : PlataformaBase
    {
        #region Propriedades do Protocolo RX
        /// <summary>
        /// Protocolo de comunicação (sempre RX).
        /// </summary>
        public override ProtocoloPlataforma Protocolo => ProtocoloPlataforma.RX;
        #endregion

        #region Construtores
        /// <summary>
        /// Inicializa uma nova instância da plataforma RX.
        /// </summary>
        public PlataformaRX()
        {
            Nome = "Plataforma RX";
            ResetarValoresPadrao();
        }

        /// <summary>
        /// Inicializa uma nova instância da plataforma RX com nome específico.
        /// </summary>
        /// <param name="nome">Nome da plataforma.</param>
        public PlataformaRX(string nome) : this()
        {
            Nome = nome;
        }
        #endregion

        #region Implementação do Processamento de Dados
        /// <summary>
        /// Desmembra os valores recebidos pelos bytes do dispositivo RX.
        /// </summary>
        /// <param name="dados">Bytes com informações de pesagem.</param>
        /// <param name="identificador">ID da plataforma (0 para dados gerais, 1-N para plataformas específicas).</param>
        /// <returns>True se houve alteração nos valores.</returns>
        public override bool ProcessarDadosRecebidos(byte[] dados, object identificador)
        {
            if (dados == null || dados.Length < 14 || identificador is not int idPlataforma)
                return false;

            var modificou = false;

            try
            {
                // Lê o peso e peso bruto
                var novoPeso = BitConverter.ToSingle(dados, 0);
                var novoPesoBruto = BitConverter.ToSingle(dados, 4);

                modificou |= AtualizarPesosSeNecessario(novoPeso, novoPesoBruto);

                // Quantidade de plataformas ou Tipo de conexão
                var valorByte8 = dados[8];
                if (idPlataforma == 0) // Dados gerais
                {
                    var novaQuantidade = valorByte8;
                    if (QuantidadePlataformas != novaQuantidade)
                    {
                        QuantidadePlataformas = novaQuantidade;
                        modificou = true;
                    }
                }
                else // Plataforma específica
                {
                    var novoTipoConexao = (TipoConexaoPlataforma)valorByte8;
                    if (TipoConexao != novoTipoConexao)
                    {
                        TipoConexao = novoTipoConexao;
                        modificou = true;
                    }
                }

                // Casas decimais
                var novasCasas = dados[9];
                if (CasasDecimais != novasCasas)
                {
                    CasasDecimais = novasCasas;
                    modificou = true;
                }

                // Processamento do Status 1 (byte 10)
                modificou |= ProcessarStatus1(dados[10]);

                // Processamento do Status 2 (byte 11)
                modificou |= ProcessarStatus2(dados[11]);

                // Processamento da bateria (bytes 12-13)
                modificou |= ProcessarDadosBateria(dados[12], dados[13]);

                if (modificou)
                {
                    UltimaLeitura = DateTime.Now;
                }

                return modificou;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar dados RX: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Métodos Privados de Processamento
        /// <summary>
        /// Processa o byte de status 1.
        /// </summary>
        /// <param name="status1">Byte de status 1.</param>
        /// <returns>True se houve alteração.</returns>
        private bool ProcessarStatus1(byte status1)
        {
            var modificou = false;

            // Extrair flags do status 1
            var novoConectada = (status1 & 1) > 0;
            var novoPesagemAferida = (status1 & (1 << 1)) > 0;
            var novoTarado = (status1 & (1 << 2)) > 0;
            var novoSobrecarga = (status1 & (1 << 3)) > 0;
            var novoSubcarga = (status1 & (1 << 4)) > 0;
            var novoPesagemEstavel = (status1 & (1 << 5)) > 0;
            var novoZero = (status1 & (1 << 6)) > 0;
            var novoCargaVivaHabilitada = (status1 & (1 << 7)) > 0;

            // Atualizar propriedades se necessário
            if (Conectada != novoConectada) { Conectada = novoConectada; modificou = true; }
            if (PesagemAferida != novoPesagemAferida) { PesagemAferida = novoPesagemAferida; modificou = true; }
            if (Tarado != novoTarado) { Tarado = novoTarado; modificou = true; }
            if (Sobrecarga != novoSobrecarga) { Sobrecarga = novoSobrecarga; modificou = true; }
            if (Subcarga != novoSubcarga) { Subcarga = novoSubcarga; modificou = true; }
            if (PesagemEstavel != novoPesagemEstavel) { PesagemEstavel = novoPesagemEstavel; modificou = true; }
            if (Zero != novoZero) { Zero = novoZero; modificou = true; }
            if (CargaVivaHabilitada != novoCargaVivaHabilitada) { CargaVivaHabilitada = novoCargaVivaHabilitada; modificou = true; }

            return modificou;
        }

        /// <summary>
        /// Processa o byte de status 2.
        /// </summary>
        /// <param name="status2">Byte de status 2.</param>
        /// <returns>True se houve alteração.</returns>
        private bool ProcessarStatus2(byte status2)
        {
            var modificou = false;

            // Extrair flags do status 2
            var novoCargaVivaFixou = (status2 & 1) > 0;
            var novoCarregandoBateria = (status2 & (1 << 1)) > 0;

            // Atualizar propriedades se necessário
            if (CargaVivaFixou != novoCargaVivaFixou) { CargaVivaFixou = novoCargaVivaFixou; modificou = true; }
            if (CarregandoBateria != novoCarregandoBateria) { CarregandoBateria = novoCarregandoBateria; modificou = true; }

            return modificou;
        }

        /// <summary>
        /// Processa os dados de bateria.
        /// </summary>
        /// <param name="byte12">Byte 12 dos dados.</param>
        /// <param name="byte13">Byte 13 dos dados.</param>
        /// <returns>True se houve alteração.</returns>
        private bool ProcessarDadosBateria(byte byte12, byte byte13)
        {
            try
            {
                // Reconstituir a tensão da bateria
                var tensaoBateria = BitConverter.ToInt16(new byte[] { byte12, byte13 }, 0);
                var novaPorcentagemBateria = CalcularPorcentagemBateria(tensaoBateria);

                if (PorcentagemBateria != novaPorcentagemBateria)
                {
                    PorcentagemBateria = novaPorcentagemBateria;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar dados de bateria: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Métodos Específicos do RX
        /// <summary>
        /// Verifica se todos os dados necessários foram lidos.
        /// </summary>
        /// <returns>True se todos os dados foram lidos.</returns>
        public bool TodosDadosLidos()
        {
            return PesagemAferida && Conectada && PesoDefinido;
        }

        /// <summary>
        /// Verifica se há alguma violação de peso.
        /// </summary>
        /// <returns>True se há violação.</returns>
        public bool TemViolacao()
        {
            return Sobrecarga || Subcarga;
        }

        /// <summary>
        /// Obtém informações específicas do protocolo RX.
        /// </summary>
        /// <returns>Dicionário com informações específicas.</returns>
        public Dictionary<string, object> ObterInformacoesRX()
        {
            var info = new Dictionary<string, object>
            {
                ["Protocolo"] = "RX",
                ["QuantidadePlataformas"] = QuantidadePlataformas,
                ["TipoConexao"] = TipoConexao.ToString(),
                ["TemViolacao"] = TemViolacao(),
                ["TodosDadosLidos"] = TodosDadosLidos()
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
        /// Reseta valores específicos do protocolo RX.
        /// </summary>
        public override void ResetarValoresPadrao()
        {
            base.ResetarValoresPadrao();
            QuantidadePlataformas = 1;
            TipoConexao = TipoConexaoPlataforma.Bluetooth;
        }

        /// <summary>
        /// Obtém informações específicas da plataforma RX.
        /// </summary>
        /// <returns>String com informações resumidas.</returns>
        public override string ObterInformacoes()
        {
            var info = base.ObterInformacoes();
            
            if (QuantidadePlataformas > 1)
            {
                info += $" - {QuantidadePlataformas} plataformas";
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