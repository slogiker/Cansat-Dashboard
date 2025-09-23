using Cansat_Dashboard.Shared;
using Cansat_Dashboard.ApiService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace Cansat_Dashboard.ApiService.Services
{
    public class TelemetryService : BackgroundService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<TelemetryService> _logger;
        private readonly string _filePath = "cansat_packets.txt";

        public TelemetryService(IHubContext<DashboardHub> hubContext, ILogger<TelemetryService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Give the app a moment to start up before reading the file
            await Task.Delay(2000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogError("Data file not found, waiting...: {FilePath}", _filePath);
                    await Task.Delay(5000, stoppingToken); // Wait 5 seconds and check again
                    continue;
                }

                try
                {
                    using var reader = new StreamReader(_filePath);
                    _logger.LogInformation("Starting to read telemetry file.");
                    while (!reader.EndOfStream && !stoppingToken.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync(stoppingToken);
                        if (TryParse(line, out var data))
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveData", data, stoppingToken);
                        }
                        await Task.Delay(1000, stoppingToken);
                    }
                    _logger.LogInformation("Finished reading telemetry file. Will restart if file changes or app restarts.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the telemetry file.");
                }
                // Wait before checking the file again from the beginning
                await Task.Delay(5000, stoppingToken);
            }
        }

        private bool TryParse(string? line, out CanSatData data)
        {
            data = new CanSatData();
            if (string.IsNullOrWhiteSpace(line)) return false;

            var parts = line.Split(',');
            if (parts.Length < 25) return false;

            try
            {
                data.PacketCount = int.Parse(parts[0]);
                data.Timestamp = long.Parse(parts[1]);
                data.BatteryVoltage = float.Parse(parts[2], CultureInfo.InvariantCulture);
                data.Temperature = float.Parse(parts[3], CultureInfo.InvariantCulture);
                data.Pressure = float.Parse(parts[4], CultureInfo.InvariantCulture);
                data.Humidity = float.Parse(parts[5], CultureInfo.InvariantCulture);
                data.VocIndex = float.Parse(parts[6], CultureInfo.InvariantCulture);
                data.UvIndex = float.Parse(parts[7], CultureInfo.InvariantCulture);
                data.Lux = float.Parse(parts[8], CultureInfo.InvariantCulture);
                data.MagX = float.Parse(parts[9], CultureInfo.InvariantCulture);
                data.MagY = float.Parse(parts[10], CultureInfo.InvariantCulture);
                data.MagZ = float.Parse(parts[11], CultureInfo.InvariantCulture);
                data.AccelX = float.Parse(parts[12], CultureInfo.InvariantCulture);
                data.AccelY = float.Parse(parts[13], CultureInfo.InvariantCulture);
                data.AccelZ = float.Parse(parts[14], CultureInfo.InvariantCulture);
                data.GyroX = float.Parse(parts[15], CultureInfo.InvariantCulture);
                data.GyroY = float.Parse(parts[16], CultureInfo.InvariantCulture);
                data.GyroZ = float.Parse(parts[17], CultureInfo.InvariantCulture);
                data.Yaw = float.Parse(parts[18], CultureInfo.InvariantCulture);
                data.Pitch = float.Parse(parts[19], CultureInfo.InvariantCulture);
                data.Roll = float.Parse(parts[20], CultureInfo.InvariantCulture);
                data.GpsLatitude = float.Parse(parts[21], CultureInfo.InvariantCulture);
                data.GpsLongitude = float.Parse(parts[22], CultureInfo.InvariantCulture);
                data.GpsAltitude = float.Parse(parts[23], CultureInfo.InvariantCulture);
                data.GpsSatellites = int.Parse(parts[24]);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse line: {Line}", line);
                return false;
            }
        }
    }
}