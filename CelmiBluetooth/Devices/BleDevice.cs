using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CelmiBluetooth.Utils;

namespace CelmiBluetooth.Devices
{
    /// <summary>
    /// Representa um dispositivo Bluetooth LE descoberto durante o scan.
    /// </summary>
    public partial class BleDevice : ObservableObject, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Nome do dispositivo BLE.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Rede))]
        [NotifyPropertyChangedFor(nameof(RedeComoNumero))]
        [NotifyPropertyChangedFor(nameof(PlataformaNumero))]
        private string name = string.Empty;

        /// <summary>
        /// Endereço MAC do dispositivo BLE.
        /// </summary>
        [ObservableProperty]
        private string address = string.Empty;

        /// <summary>
        /// Força do sinal RSSI em dBm.
        /// </summary>
        [ObservableProperty]
        private int rssi;

        /// <summary>
        /// Indica se o dispositivo está conectado.
        /// </summary>
        [ObservableProperty]
        private bool isConnected;

        /// <summary>
        /// Versão do firmware do dispositivo.
        /// </summary>
        [ObservableProperty]
        private string firmwareVersion = string.Empty;

        /// <summary>
        /// Status do dispositivo, que pode ser sobrescrito por classes derivadas.
        /// </summary>
        public virtual string Status => IsConnected ? "Conectado" : "Desconectado";

        /// <summary>
        /// Referência ao dispositivo peripheral do Shiny para operações de conexão.
        /// </summary>
        public object? Peripheral { get; set; }

        /// <summary>
        /// Verifica se o dispositivo é um RX (contém "BLE-RX" no nome).
        /// </summary>
        public bool IsRXDevice => !string.IsNullOrEmpty(Name) && Name.Contains("BLE-RX", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Verifica se o dispositivo é um TX (contém "BLE-TX" no nome).
        /// </summary>
        public bool IsTXDevice => !string.IsNullOrEmpty(Name) && Name.Contains("BLE-TX", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Extrai a versão do firmware do nome do dispositivo RX (formato "F337").
        /// </summary>
        public string ExtractFirmwareVersion()
        {
            return CelmiDeviceUtils.ExtractFirmwareVersion(Name);
        }

        /// <summary>
        /// Extrai a rede do nome do dispositivo (ex: "R4" retorna "4").
        /// Funciona tanto para dispositivos RX quanto TX.
        /// </summary>
        public string? Rede
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || (!IsRXDevice && !IsTXDevice))
                    return null;

                // Procurar por padrão R seguido de caractere, mas não como parte de BLE-RX ou BLE-TX
                // Use lookbehind negativo para garantir que não esteja precedido por "BLE-"
                var match = Regex.Match(Name, @"(?<!BLE-)R([0-9A-Za-z:;<>=?@\[\\\]^_`{|}~])");
                return match.Success ? match.Groups[1].Value : null;
            }
        }

        /// <summary>
        /// Converte a rede (letra ou número) para seu valor numérico.
        /// </summary>
        public int RedeComoNumero
        {
            get
            {
                if (string.IsNullOrEmpty(Rede))
                    return -1;

                switch (Rede)
                {
                    case ":": return 10;
                    case ";": return 11;
                    case "<": return 12;
                    case "=": return 13;
                    case ">": return 14;
                    case "?": return 15;
                    case "@": return 16;
                    case "A": return 17;
                    case "B": return 18;
                    case "C": return 19;
                    case "D": return 20;
                    case "E": return 21;
                    case "F": return 22;
                    case "G": return 23;
                    case "H": return 24;
                    case "I": return 25;
                    case "J": return 26;
                    case "K": return 27;
                    case "L": return 28;
                    case "M": return 29;
                    case "N": return 30;
                    case "O": return 31;
                    case "P": return 32;
                    case "Q": return 33;
                    case "R": return 34;
                    case "S": return 35;
                    case "T": return 36;
                    case "U": return 37;
                    case "V": return 38;
                    case "W": return 39;
                    case "X": return 40;
                    case "Y": return 41;
                    case "Z": return 42;
                    case "[": return 43;
                    case "\\": return 44;
                    case "]": return 45;
                    case "^": return 46;
                    case "_": return 47;
                    case "`": return 48;
                    case "a": return 49;
                    case "b": return 50;
                    case "c": return 51;
                    case "d": return 52;
                    case "e": return 53;
                    case "f": return 54;
                    case "g": return 55;
                    case "h": return 56;
                    case "i": return 57;
                    case "j": return 58;
                    case "k": return 59;
                    case "l": return 60;
                    case "m": return 61;
                    case "n": return 62;
                    case "o": return 63;
                    case "p": return 64;
                    case "q": return 65;
                    case "r": return 66;
                    case "s": return 67;
                    case "t": return 68;
                    case "u": return 69;
                    case "v": return 70;
                    case "w": return 71;
                    case "x": return 72;
                    case "y": return 73;
                    case "z": return 74;
                    case "{": return 75;
                    case "|": return 76;
                    case "}": return 77;
                    case "~": return 78;
                    default:
                        if (int.TryParse(Rede, out var result))
                            return result;
                        return -1;
                }
            }
        }

        /// <summary>
        /// Extrai o número de plataformas do nome do dispositivo RX (ex: "P4" retorna 4).
        /// </summary>
        public int? Plataformas
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || !IsRXDevice)
                    return null;

                // Procurar por padrão P seguido de número (ex: P4, P8, etc.)
                var match = Regex.Match(Name, @"P(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var platforms))
                    return platforms;
                
                return null;
            }
        }

        /// <summary>
        /// Número da plataforma extraído do nome do dispositivo (ex: "P2" retorna 2).
        /// Funciona tanto para dispositivos RX quanto TX.
        /// </summary>
        public int? PlataformaNumero
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || (!IsRXDevice && !IsTXDevice))
                    return null;
                // Procurar por padrão P seguido de número (ex: P2, P8, etc.)
                var match = Regex.Match(Name, @"P(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var numero))
                    return numero;
                return null;
            }
        }

        /// <summary>
        /// Informação formatada sobre rede e plataformas para dispositivos RX.
        /// </summary>
        public string RXInfo
        {
            get
            {
                if (!IsRXDevice)
                    return Address;

                var info = new List<string>();
                
                if (RedeComoNumero > 0)
                    info.Add($"Rede {RedeComoNumero}");
                
                if (Plataformas.HasValue)
                    info.Add($"Plataformas {Plataformas.Value}");

                return info.Count > 0 ? string.Join(" | ", info) : Address;
            }
        }
        
        /// <summary>
        /// Descrição da força do sinal.
        /// </summary>
        public string SignalDescription
        {
            get
            {
                if (Rssi >= -50) return "Excelente";
                if (Rssi >= -60) return "Muito Boa";
                if (Rssi >= -70) return "Boa";
                if (Rssi >= -80) return "Fraco";
                return "Muito Fraca";
            }
        }

        /// <summary>
        /// Cor representando a força do sinal.
        /// </summary>
        public string SignalColor
        {
            get
            {
                return Rssi switch
                {
                    >= -50 => "#4CAF50",    // Verde
                    >= -60 => "#8BC34A",    // Verde claro
                    >= -70 => "#FF9800",    // Laranja
                    >= -80 => "#FF5722",    // Laranja escuro
                    _ => "#F44336"          // Vermelho
                };
            }
        }

        #region IDisposable Implementation

        /// <summary>
        /// Libera todos os recursos utilizados pelo BleDevice.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera os recursos gerenciados e não-gerenciados utilizados pelo BleDevice.
        /// </summary>
        /// <param name="disposing">true se chamado de Dispose(); false se chamado do finalizador.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    // Limpar referência ao Peripheral se for IDisposable
                    if (Peripheral is IDisposable disposablePeripheral)
                    {
                        disposablePeripheral.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao fazer dispose do Peripheral: {ex.Message}");
                }
                finally
                {
                    Peripheral = null;
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Finalizador para garantir que os recursos sejam liberados mesmo se Dispose não for chamado.
        /// </summary>
        ~BleDevice()
        {
            Dispose(false);
        }

        #endregion
    }
}