using Cansat_Dashboard.ApiService.Data;
using Cansat_Dashboard.ApiService.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Cansat_Dashboard.ApiService.Services;

public class TelemetryService : BackgroundService
{
    private readonly IHubContext<DashboardHub> _hubContext;

    public TelemetryService(IHubContext<DashboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a moment for the services to start up
        await Task.Delay(2000, stoppingToken);

        var packetLines = await File.ReadAllLinesAsync("cansat_packets.txt", stoppingToken);
        var lineIndex = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (packetLines.Length > 0)
            {
                // Loop through the file
                var line = packetLines[lineIndex];
                var data = ParsePacket(line);
                if (data != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveSensorData", data, stoppingToken);
                }
                lineIndex = (lineIndex + 1) % packetLines.Length;
            }

            await Task.Delay(1000, stoppingToken); // Update every 1 second
        }
    }

    private CanSatData? ParsePacket(string hexLine)
    {
        try
        {
            var hexChars = hexLine.Replace(" ", "");
            if (hexChars.Length != 200) return null;

            byte[] bytes = Enumerable.Range(0, hexChars.Length)
                                     .Where(x => x % 2 == 0)
                                     .Select(x => Convert.ToByte(hexChars.Substring(x, 2), 16))
                                     .ToArray();

            var data = new CanSatData
            {
                MissionTime = BitConverter.ToSingle(bytes, 7).ToString("F0"),
                Altitude = (int)BitConverter.ToSingle(bytes, 15),
                Pressure = BitConverter.ToSingle(bytes, 19),
                Temperature = BitConverter.ToSingle(bytes, 23),
                Humidity = BitConverter.ToSingle(bytes, 27),
                VocIndex = (int)BitConverter.ToSingle(bytes, 35),
                Latitude = BitConverter.ToSingle(bytes, 43),
                Longitude = BitConverter.ToSingle(bytes, 47),
                GpsAltitude = BitConverter.ToInt32(bytes, 51),
                SatelliteCount = BitConverter.ToInt32(bytes, 55),
                Pitch = BitConverter.ToSingle(bytes, 63),
                Roll = BitConverter.ToSingle(bytes, 67),
                Yaw = BitConverter.ToSingle(bytes, 71),
                Acceleration = new Vector3
                {
                    X = BitConverter.ToSingle(bytes, 75),
                    Y = BitConverter.ToSingle(bytes, 79),
                    Z = BitConverter.ToSingle(bytes, 83)
                },
                MagneticField = new Vector3
                {
                    X = BitConverter.ToSingle(bytes, 87),
                    Y = BitConverter.ToSingle(bytes, 91),
                    Z = BitConverter.ToSingle(bytes, 95)
                },
                PictureStatus = "OK"
            };
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing packet: {ex.Message}");
            return null;
        }
    }
}
