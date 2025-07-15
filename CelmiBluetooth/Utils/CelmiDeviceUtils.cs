using System.Text.RegularExpressions;

namespace CelmiBluetooth.Utils
{
    /// <summary>
    /// Utilit�rios para manipula��o de informa��es de dispositivos Celmi.
    /// </summary>
    public static class CelmiDeviceUtils
    {
        /// <summary>
        /// Extrai a vers�o do firmware do nome do dispositivo.
        /// Suporta o formato "F337" (ex: "BLE-RX R4 P6 F337 S00404").
        /// </summary>
        /// <param name="deviceName">Nome do dispositivo</param>
        /// <returns>Vers�o do firmware (ex: "F337") ou string vazia se n�o encontrar</returns>
        public static string ExtractFirmwareVersion(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                return string.Empty;

            try
            {
                // Formato: "BLE-RX R4 P6 F337 S00404" onde F337 � o firmware
                var match = Regex.Match(deviceName, @"\bP\d+\s+F(\d+)");
                
                if (match.Success)
                {
                    return $"F{match.Groups[1].Value}";
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}