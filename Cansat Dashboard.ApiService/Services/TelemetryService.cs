using Cansat_Dashboard.ApiService.Hubs;
using Cansat_Dashboard.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cansat_Dashboard.ApiService.Services
{
    public class TelemetryService : BackgroundService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<TelemetryService> _logger;
        private readonly string _inputFilePath = "cansat_packets.txt";
        private readonly string _outputFilePath = "output.csv";

        public TelemetryService(IHubContext<DashboardHub> hubContext, ILogger<TelemetryService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(2000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!File.Exists(_inputFilePath))
                {
                    _logger.LogError("Data file not found, waiting...: {FilePath}", _inputFilePath);
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                try
                {
                    await ProcessTelemetryFile(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the telemetry file.");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task ProcessTelemetryFile(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting to read telemetry file.");
            using var writer = new StreamWriter(_outputFilePath, false, Encoding.UTF8);
            await writer.WriteLineAsync("Header,Payload,CRC,PacketCount,Timestamp,BatteryVoltage,Temperature,Pressure,Humidity,VocIndex,UvIndex,Lux,MagX,MagY,MagZ,AccelX,AccelY,AccelZ,GyroX,GyroY,GyroZ,Yaw,Pitch,Roll,GpsLatitude,GpsLongitude,GpsAltitude,GpsSatellites");

            var fileContent = await File.ReadAllTextAsync(_inputFilePath, stoppingToken);
            var cleanContent = Regex.Replace(fileContent, @"\s+", " ").Trim();
            var hexValues = cleanContent.Split(' ');
            var byteData = hexValues.Select(hex => byte.Parse(hex, NumberStyles.HexNumber)).ToArray();

            var state = State.WAIT_HEADER1;
            var payload = new List<byte>();
            byte length = 0;
            byte crc = 0;


            foreach (var b in byteData)
            {
                switch (state)
                {
                    case State.WAIT_HEADER1:
                        if (b == 0xAA) state = State.WAIT_HEADER2;
                        break;
                    case State.WAIT_HEADER2:
                        if (b == 0x55) state = State.READ_LENGTH;
                        else state = State.WAIT_HEADER1;
                        break;
                    case State.READ_LENGTH:
                        length = b;
                        payload.Clear();
                        state = State.READ_PAYLOAD;
                        break;
                    case State.READ_PAYLOAD:
                        payload.Add(b);
                        if (payload.Count == length) state = State.READ_CRC;
                        break;
                    case State.READ_CRC:
                        crc = b;
                        if (ComputeCRC8(payload.ToArray(), length) == crc)
                        {
                            if (TryParsePayload(payload.ToArray(), out var data))
                            {
                                await _hubContext.Clients.All.SendAsync("ReceiveData", data, stoppingToken);
                                var csvLine = ToCsv(data, payload.ToArray(), crc);
                                await writer.WriteLineAsync(csvLine);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("CRC check failed. Packet discarded.");
                        }
                        state = State.WAIT_HEADER1;
                        break;
                }
            }

            _logger.LogInformation("Finished reading telemetry file.");
        }

        private static bool TryParsePayload(byte[] payload, out CanSatData data)
        {
            data = new CanSatData();
            if (payload.Length < 100) return false;

            try
            {
                data.PacketCount = BitConverter.ToInt32(payload, 0);
                data.Timestamp = BitConverter.ToInt64(payload, 4);
                data.BatteryVoltage = BitConverter.ToSingle(payload, 12);
                data.Temperature = BitConverter.ToSingle(payload, 16);
                data.Pressure = BitConverter.ToSingle(payload, 20);
                data.Humidity = BitConverter.ToSingle(payload, 24);
                data.VocIndex = BitConverter.ToSingle(payload, 28);
                data.UvIndex = BitConverter.ToSingle(payload, 32);
                data.Lux = BitConverter.ToSingle(payload, 36);
                data.MagX = BitConverter.ToSingle(payload, 40);
                data.MagY = BitConverter.ToSingle(payload, 44);
                data.MagZ = BitConverter.ToSingle(payload, 48);
                data.AccelX = BitConverter.ToSingle(payload, 52);
                data.AccelY = BitConverter.ToSingle(payload, 56);
                data.AccelZ = BitConverter.ToSingle(payload, 60);
                data.GyroX = BitConverter.ToSingle(payload, 64);
                data.GyroY = BitConverter.ToSingle(payload, 68);
                data.GyroZ = BitConverter.ToSingle(payload, 72);
                data.Yaw = BitConverter.ToSingle(payload, 76);
                data.Pitch = BitConverter.ToSingle(payload, 80);
                data.Roll = BitConverter.ToSingle(payload, 84);
                data.GpsLatitude = BitConverter.ToSingle(payload, 88);
                data.GpsLongitude = BitConverter.ToSingle(payload, 92);
                data.GpsAltitude = BitConverter.ToSingle(payload, 96);
                data.GpsSatellites = BitConverter.ToInt32(payload, 100);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ToCsv(CanSatData data, byte[] payload, byte crc)
        {
            var header = "AA55";
            var payloadString = BitConverter.ToString(payload).Replace("-", "");
            var crcString = crc.ToString("X2");

            return $"{header},{payloadString},{crcString},{data.PacketCount},{data.Timestamp},{data.BatteryVoltage},{data.Temperature},{data.Pressure},{data.Humidity},{data.VocIndex},{data.UvIndex},{data.Lux},{data.MagX},{data.MagY},{data.MagZ},{data.AccelX},{data.AccelY},{data.AccelZ},{data.GyroX},{data.GyroY},{data.GyroZ},{data.Yaw},{data.Pitch},{data.Roll},{data.GpsLatitude},{data.GpsLongitude},{data.GpsAltitude},{data.GpsSatellites}";
        }

        private static byte ComputeCRC8(byte[] data, int length)
        {
            byte crc = 0;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x80) != 0)
                        crc = (byte)((crc << 1) ^ 0x07);
                    else
                        crc <<= 1;
                }
            }
            return crc;
        }

        private enum State { WAIT_HEADER1, WAIT_HEADER2, READ_LENGTH, READ_PAYLOAD, READ_CRC };
    }
}