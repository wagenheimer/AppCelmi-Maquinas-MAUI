using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CelmiBluetooth
{
    public static class CelmiUUID
    {
        public static class Service
        {
            public const string SERVICO_INFORMACOES = "97d8ccb6-b6b6-4bc8-a7ba-5b4ecb9e4f62";
            public const string SERVICO_DE_CONFIGURACOES = "1f8c745e-f12f-43ca-aa95-09f3f11810a3";
            public const string SERVICO_DE_COMANDOS = "6008f2f8-7ec2-4e40-93a7-320832d7cc89";
            public const string SERVICO_PARA_NOVOS_UUIDS = "030ecd95-d0e7-41a6-8d66-89b55b68fa70";

            public const string DISPOSITIVORX = "000030ff-0000-1000-8000-00805f9b34fb";

            public const string LEAPSERVICE = "0000fff0-0000-1000-8000-00805f9b34fb";
        }

        public static class Characteristic
        {
            public const string NOME_DO_DISPOSITIVO = "00002a00-0000-1000-8000-00805f9b34fb";

            public const string HEART_RATE_MEASUREMENT = "00002a37-0000-1000-8000-00805f9b34fb";
            public const string MANUFACTURER_STRING = "00002a29-0000-1000-8000-00805f9b34fb";
            public const string MODEL_NUMBER_STRING = "00002a24-0000-1000-8000-00805f9b34fb";
            public const string FIRMWARE_REVISION_STRING = "00002a26-0000-1000-8000-00805f9b34fb";
            public const string APPEARANCE = "00002a01-0000-1000-8000-00805f9b34fb";
            public const string BODY_SENSOR_LOCATION = "00002a38-0000-1000-8000-00805f9b34fb";
            public const string BATTERY_LEVEL = "00002a19-0000-1000-8000-00805f9b34fb";

            /**
             * Serviço de Informações
             */
            public const string VALOR_ADC = "d37cb4f6-5629-46a6-84a8-c3ce77694a95";

            public const string TENSAO_BATERIA = "caf067c0-cb45-4098-b811-fcdd41b047f8";
            public const string PESO = "875607c1-1f96-449c-b4d5-14dab172d5e6";
            public const string AFERIDO = "8e82aad1-11da-42a3-8e5d-9302800ea3bc";
            public const string TARADO = "e325ec28-ae18-4796-9594-1d5c7007eedb";
            public const string SOBRECARGA = "fd886a00-dec1-4a23-899f-56967da279dd";
            public const string SOBCARGA = "1a02a06d-17d4-4d51-8912-a277b924aee3";
            public const string ESTAVEL = "588b6cfa-1d0a-4a91-8f0a-811f10a6c1d7";
            public const string NUMERO_PLATAFORMA = "1311b765-0726-4bf9-a894-e21fe4a0dacc";
            public const string NUMERO_REDE = "509ad5d8-e4cf-484c-b97a-00e538859f68";
            public const string VERSAO_FIRMWARE = "06b45a96-abfd-426d-aaff-06cbb47c3e20";
            public const string NUMERO_SERIE = "4e31ba1c-e197-46b4-a683-023cc81353a5";
            public const string CARREGANDO_BATERIA = "3710a04b-130a-40f6-9800-72a280160c7a";

            /**
             * Serviço de Configurações
             */
            public const string CAPACIDADE = "910c2ca8-883a-4a7e-8e1b-1b970bce0407";

            public const string SPAN = "98e0856d-57fc-41ba-981c-ae9e57135a8f";
            public const string CAL_ZERO = "f6d64fde-476d-40f5-9525-f442d298e306";
            public const string CAL_SPAN = "eae1e80a-7543-4e65-9aec-3200529071f0";
            public const string PASSO = "1999b84f-6d79-4f3d-8918-3c010225c05b";
            public const string CASAS_DECIMAIS = "517a516b-eddd-4bbe-8242-29c5d74a813c";
            public const string TEMPO_AUTO_ZERO = "7ae2429c-424c-400c-b904-e667c966b73a";
            public const string LIMITE_AUTO_ZERO = "fd78b24c-64e9-4537-838b-49b84dac71a7";
            public const string LIMITE_ZERO_MANUAL = "3349de74-0301-4668-908c-9b9ab823058f";
            public const string LIMITE_ZERO_INICIAL = "e69cc1ff-55bf-465a-b639-dc44a6027367";
            public const string FILTRO_DIGITAL = "27ab77c7-ce69-40a1-9402-29aed3e6502e";

            /**
             * Serviço de comandos
             */
            public const string CMD_ZERO = "45936085-769a-4982-abae-5ee7f39ce602";

            public const string CMD_TARA = "430d7685-1408-4896-96bc-d96a0233ecfb";
            public const string CMD_RESET_SETUP = "f73c0799-525e-48db-86b4-b0406deba855";

            /**
             * Serviço para novos UUIDS
             */
            public const string UMA_DIVISAO_ADC = "89b03c4c-0444-4f14-b243-03ba4c1a636a";

            public const string CAPACIDADE_ADC = "2844ad28-a09b-48ca-b466-f4cb788cc73c";
            public const string ADDIFF = "7198a706-1626-4d17-9ffa-efdbdd94a46d";
            public const string DIVISOES = "761303af-a5b9-452d-bd73-3b56dd517780";
            public const string CAPACIDADE_BATERIA = "1fe14ed5-db01-4876-ae9a-fc7a242771ee";
            public const string CORRENTE_BATERIA = "4830336d-d4d9-4341-bc19-3dd5406b5749";
            public const string DESVIO_ZERO_MINIMO = "2016d66a-bc6d-42f3-97f0-ddb88504c324";
            public const string MAJOR = "338fd9b6-62d5-4545-bbc8-8b607d555d37";
            public const string MINOR = "39b37f26-149d-4559-bdc8-3cb9c89695c0";
            public const string PATCH = "06573c6a-3bb9-4659-b822-f1208b305002";
            public const string BUILD = "fd7a68cd-f97d-40e9-8fbd-6cfcc3eafbc8";
            public const string REDE_BIT = "1e144401-f3c0-4d4b-b129-46e02d263bff";
            public const string PLATAFORMA_BIT = "60ea29d4-9403-4b84-9f20-52a675eddccb";
            public const string PESO_BRUTO = "8dada1c0-d0c9-4c57-8503-afaeea8523bc";
            public const string CV_HAB = "7b4eed23-cb23-4787-b2c8-fa0f3df16e3d";
            public const string CV_TEMPO = "e45294ea-a4f9-4457-8eee-a664a2fe1f1a";
            public const string CV_LIMITE = "cb19bfe9-db18-4032-b550-44e7eb147b07";
            public const string CV_FIXOU = "c20424c3-2a8b-4bd4-897e-6d61570a25cd";
            public const string PESO_TARA = "1e300b87-c7ee-4b9e-9175-855f30e76239";
            public const string PESDIN_HAB = "564ed0f9-6d4f-4f90-8296-2f1b77211efb";
            public const string PESDIN_PESOACC = "dfc0d92a-186e-41a5-9bca-8c9fcc3cd23c";
            public const string PESDIN_EIXODATA = "ba5b2f68-aecf-4b19-96d6-a581d7b05f9c";


            //CSP10B-RX
            public const string RXGeral = "dd923643-3af6-4cea-b5b9-7290c2f2fcac";
            public const string RXPlat1 = "f657a874-a455-42e0-9ee6-711ab1bb3cbe";
            public const string RXPlat2 = "66f8f341-5fbf-44c9-acc5-d55c8538bd8f";
            public const string RXPlat3 = "fed4a24e-18e2-44be-bdc5-2d469ff28c32";
            public const string RXPlat4 = "f7ae678f-c39e-4edc-b154-3be135f6da6e";
            public const string RXPlat5 = "9dd611da-8c21-40f1-95d9-02a66402a72e";
            public const string RXPlat6 = "6d8a075f-b1fb-41a9-82c2-d359b48a5211";
            public const string RXPlat7 = "04a5189d-f6b8-48f1-8ddd-187fa6482e2c";
            public const string RXPlat8 = "e94e0e08-83e3-4285-b5e7-587c0c4ee38e";
            public const string RXPlat9 = "45be7307-7b9f-411d-8ebe-18c74891f836";
            public const string RXPlat10 = "220ff832-a10e-4c5e-996a-29ce9bf5011a";
            public const string RXPlat11 = "e3ec7ec4-5b79-470e-93e5-9bd348922b31";
            public const string RXPlat12 = "1b22f3a4-baa0-4719-b04d-6795c1a81e54";


            //LEAP
            public const string LEAPNotify = "0000fff1-0000-1000-8000-00805f9b34fb";
            public const string LEAPWrite = "0000fff2-0000-1000-8000-00805f9b34fb";
        }

        private static class Descriptor
        {
            public const string CHAR_CLIENT_CONFIG = "00002902-0000-1000-8000-00805f9b34fb";
        }

        public static string UuidName(this string uuid)
        {
            return uuid switch
            {
                Descriptor.CHAR_CLIENT_CONFIG => nameof(Descriptor.CHAR_CLIENT_CONFIG),

                Characteristic.HEART_RATE_MEASUREMENT => nameof(Characteristic.HEART_RATE_MEASUREMENT),
                Characteristic.MANUFACTURER_STRING => nameof(Characteristic.MANUFACTURER_STRING),
                Characteristic.MODEL_NUMBER_STRING => nameof(Characteristic.MODEL_NUMBER_STRING),
                Characteristic.FIRMWARE_REVISION_STRING => nameof(Characteristic.FIRMWARE_REVISION_STRING),
                Characteristic.APPEARANCE => nameof(Characteristic.APPEARANCE),
                Characteristic.BODY_SENSOR_LOCATION => nameof(Characteristic.BODY_SENSOR_LOCATION),
                Characteristic.BATTERY_LEVEL => nameof(Characteristic.BATTERY_LEVEL),
                Characteristic.VALOR_ADC => nameof(Characteristic.VALOR_ADC),
                Characteristic.TENSAO_BATERIA => nameof(Characteristic.TENSAO_BATERIA),
                Characteristic.PESO => nameof(Characteristic.PESO),
                Characteristic.AFERIDO => nameof(Characteristic.AFERIDO),
                Characteristic.TARADO => nameof(Characteristic.TARADO),
                Characteristic.SOBRECARGA => nameof(Characteristic.SOBRECARGA),
                Characteristic.SOBCARGA => nameof(Characteristic.SOBCARGA),
                Characteristic.ESTAVEL => nameof(Characteristic.ESTAVEL),
                Characteristic.NUMERO_PLATAFORMA => nameof(Characteristic.NUMERO_PLATAFORMA),
                Characteristic.NUMERO_REDE => nameof(Characteristic.NUMERO_REDE),
                Characteristic.VERSAO_FIRMWARE => nameof(Characteristic.VERSAO_FIRMWARE),
                Characteristic.NUMERO_SERIE => nameof(Characteristic.NUMERO_SERIE),
                Characteristic.CARREGANDO_BATERIA => nameof(Characteristic.CARREGANDO_BATERIA),
                Characteristic.CAPACIDADE => nameof(Characteristic.CAPACIDADE),
                Characteristic.SPAN => nameof(Characteristic.SPAN),
                Characteristic.CAL_ZERO => nameof(Characteristic.CAL_ZERO),
                Characteristic.CAL_SPAN => nameof(Characteristic.CAL_SPAN),
                Characteristic.PASSO => nameof(Characteristic.PASSO),
                Characteristic.CASAS_DECIMAIS => nameof(Characteristic.CASAS_DECIMAIS),
                Characteristic.TEMPO_AUTO_ZERO => nameof(Characteristic.TEMPO_AUTO_ZERO),
                Characteristic.LIMITE_AUTO_ZERO => nameof(Characteristic.LIMITE_AUTO_ZERO),
                Characteristic.LIMITE_ZERO_MANUAL => nameof(Characteristic.LIMITE_ZERO_MANUAL),
                Characteristic.LIMITE_ZERO_INICIAL => nameof(Characteristic.LIMITE_ZERO_INICIAL),
                Characteristic.FILTRO_DIGITAL => nameof(Characteristic.FILTRO_DIGITAL),
                Characteristic.CMD_ZERO => nameof(Characteristic.CMD_ZERO),
                Characteristic.CMD_TARA => nameof(Characteristic.CMD_TARA),
                Characteristic.CMD_RESET_SETUP => nameof(Characteristic.CMD_RESET_SETUP),
                Characteristic.UMA_DIVISAO_ADC => nameof(Characteristic.UMA_DIVISAO_ADC),
                Characteristic.CAPACIDADE_ADC => nameof(Characteristic.CAPACIDADE_ADC),
                Characteristic.ADDIFF => nameof(Characteristic.ADDIFF),
                Characteristic.DIVISOES => nameof(Characteristic.DIVISOES),
                Characteristic.CAPACIDADE_BATERIA => nameof(Characteristic.CAPACIDADE_BATERIA),
                Characteristic.CORRENTE_BATERIA => nameof(Characteristic.CORRENTE_BATERIA),
                Characteristic.DESVIO_ZERO_MINIMO => nameof(Characteristic.DESVIO_ZERO_MINIMO),
                Characteristic.MAJOR => nameof(Characteristic.MAJOR),
                Characteristic.MINOR => nameof(Characteristic.MINOR),
                Characteristic.PATCH => nameof(Characteristic.PATCH),
                Characteristic.BUILD => nameof(Characteristic.BUILD),
                Characteristic.REDE_BIT => nameof(Characteristic.REDE_BIT),
                Characteristic.PLATAFORMA_BIT => nameof(Characteristic.PLATAFORMA_BIT),
                Characteristic.PESO_BRUTO => nameof(Characteristic.PESO_BRUTO),
                Characteristic.CV_HAB => nameof(Characteristic.CV_HAB),
                Characteristic.CV_TEMPO => nameof(Characteristic.CV_TEMPO),
                Characteristic.CV_LIMITE => nameof(Characteristic.CV_LIMITE),
                Characteristic.CV_FIXOU => nameof(Characteristic.CV_FIXOU),
                Characteristic.PESO_TARA => nameof(Characteristic.PESO_TARA),
                Characteristic.PESDIN_HAB => nameof(Characteristic.PESDIN_HAB),
                Characteristic.PESDIN_PESOACC => nameof(Characteristic.PESDIN_PESOACC),
                Characteristic.PESDIN_EIXODATA => nameof(Characteristic.PESDIN_EIXODATA),

                Characteristic.RXGeral => nameof(Characteristic.RXGeral),
                Characteristic.RXPlat1 => nameof(Characteristic.RXPlat1),
                Characteristic.RXPlat2 => nameof(Characteristic.RXPlat2),
                Characteristic.RXPlat3 => nameof(Characteristic.RXPlat3),
                Characteristic.RXPlat4 => nameof(Characteristic.RXPlat4),
                Characteristic.RXPlat5 => nameof(Characteristic.RXPlat5),
                Characteristic.RXPlat6 => nameof(Characteristic.RXPlat6),
                Characteristic.RXPlat7 => nameof(Characteristic.RXPlat7),
                Characteristic.RXPlat8 => nameof(Characteristic.RXPlat8),
                Characteristic.RXPlat9 => nameof(Characteristic.RXPlat9),
                Characteristic.RXPlat10 => nameof(Characteristic.RXPlat10),
                Characteristic.RXPlat11 => nameof(Characteristic.RXPlat11),
                Characteristic.RXPlat12 => nameof(Characteristic.RXPlat12),
                _ => "Unknown"
            };
        }

        // For backward compatibility - keep this for any code still using the old name
        public static string UdidName(this string uuid) => UuidName(uuid);
    }
}
