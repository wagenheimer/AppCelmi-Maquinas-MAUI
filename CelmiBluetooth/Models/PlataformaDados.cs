using CommunityToolkit.Mvvm.ComponentModel;


namespace CelmiBluetooth.Models
{

    /// <summary>
    /// ViewModel para os dados de peso de uma plataforma.
    /// </summary>
    public partial class PlataformaDados : ObservableObject
    {
        /// <summary>
        /// ID da plataforma.
        /// </summary>
        [ObservableProperty]
        private int _platformId;

        /// <summary>
        /// Descrição da plataforma.
        /// </summary>
        [ObservableProperty]
        private string _description;

        /// <summary>
        /// Peso formatado.
        /// </summary>
        [ObservableProperty]
        private string _formattedWeight;

        /// <summary>
        /// Indica se o peso está estável.
        /// </summary>
        [ObservableProperty]
        private bool _isStable;

        /// <summary>
        /// Peso atual.
        /// </summary>
        [ObservableProperty]
        private float _weight;

        /// <summary>
        /// Peso bruto.
        /// </summary>
        [ObservableProperty]
        private float _grossWeight;

        /// <summary>
        /// Indica se a plataforma está conectada.
        /// </summary>
        [ObservableProperty]
        private bool _isConnected;

        /// <summary>
        /// Data e hora da última atualização.
        /// </summary>
        [ObservableProperty]
        private DateTime _lastUpdate = DateTime.Now;

        /// <summary>
        /// Porcentagem da bateria da plataforma.
        /// </summary>
        [ObservableProperty]
        private int _batteryPercentage;

        /// <summary>
        /// Construtor da PlatformWeightViewModel.
        /// </summary>
        public PlataformaDados(int platformId, string description, string formattedWeight,
            bool isStable, float weight, float grossWeight, bool isConnected, int batteryPercentage)
        {
            _platformId = platformId;
            _description = description;
            _formattedWeight = formattedWeight;
            _isStable = isStable;
            _weight = weight;
            _grossWeight = grossWeight;
            _isConnected = isConnected;
            _batteryPercentage = batteryPercentage;
            _lastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Atualiza os dados da plataforma.
        /// </summary>
        public void Update(int platformId, string description, string formattedWeight,
            bool isStable, float weight, float grossWeight, bool isConnected, int batteryPercentage)
        {
            PlatformId = platformId;
            Description = description;
            FormattedWeight = formattedWeight;
            IsStable = isStable;
            Weight = weight;
            GrossWeight = grossWeight;
            IsConnected = isConnected;
            BatteryPercentage = batteryPercentage;
            LastUpdate = DateTime.Now;
        }
    }
}