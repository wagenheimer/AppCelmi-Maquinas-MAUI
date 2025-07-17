using System.Collections.ObjectModel;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CelmiBluetooth.Models
{
    public enum TipoDispositivo
    {
        Desconhecido,
        RXFisico,
        RXVirtual,
        TX
    }

    public enum StatusPlataforma
    {
        Desconhecido,
        Conectada,
        Desconectada,
        Sobrecarga,
        Erro
    }

    public enum TipoViolacaoPeso
    {
        Normal,
        Sobrecarga,
        Subcarga
    }

    public class PesoAtualizadoEventArgs : EventArgs
    {
        public int IdPlataforma { get; set; }
        public decimal Peso { get; set; }
        public DateTime TimestampAtualizacao { get; set; }
        public bool Estavel { get; set; }
        public TipoViolacaoPeso TipoViolacao { get; set; }
    }

    public class StatusConexaoEventArgs : EventArgs
    {
        public bool Conectado { get; set; }
        public string? MensagemErro { get; set; }
        public DateTime TimestampMudanca { get; set; }
    }

    public partial class DadosPlataforma : ObservableObject
    {
        [ObservableProperty]
        private int _idPlataforma;

        [ObservableProperty]
        private string _descricao = string.Empty;

        [ObservableProperty]
        private decimal _pesoAtual;

        [ObservableProperty]
        private bool _estavel;

        [ObservableProperty]
        private StatusPlataforma _status;

        public string PesoFormatado =>
            Status == StatusPlataforma.Desconectada ? "Desconectado" :
            $"{PesoAtual:F2} kg";
    }

    public interface IDispositivoPesagem : INotifyPropertyChanged, IDisposable
    {
        string Nome { get; }
        string Status { get; }
        bool Conectado { get; }
        TipoDispositivo Tipo { get; }
        int TotalPlataformas { get; }
        decimal PesoTotal { get; }

        Task<bool> ConectarAsync();
        Task DesconectarAsync();
        string ObterPesoTotalFormatado();
        Task LerValoresIniciaisAsync();
        Task LerValoresManuaisNoIntervalo();

        Task IniciaLeituraValoresManuaisAsync();

    }

    public interface IPlataformaPesagem
    {
        int IdPlataforma { get; }
        string Descricao { get; }
        decimal PesoAtual { get; }
        bool Estavel { get; }
        StatusPlataforma Status { get; }

        event EventHandler<PesoAtualizadoEventArgs> PesoAtualizado;
        event EventHandler<StatusConexaoEventArgs> StatusConexaoAlterado;

        Task AtualizarPesoAsync(decimal novoPeso, bool estavel);
        Task AlterarStatusAsync(StatusPlataforma novoStatus);
    }
}