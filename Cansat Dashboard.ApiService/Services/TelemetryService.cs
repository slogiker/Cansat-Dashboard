using Cansat_Dashboard.ApiService;
using Cansat_Dashboard.ApiService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cansat_Dashboard.ApiService.Services;

public class TelemetryService(IHubContext<DashboardHub> hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var reader = new StreamReader("cansat_packets.txt");

        while (!stoppingToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(stoppingToken);
            if (line is null)
            {
                // Reset stream
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                continue;
            }

            var data = Parse(line);
            await hub.Clients.All.SendAsync("ReceiveTelemetry", data, cancellationToken: stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private CanSatData Parse(string hexData)
    {
        // Remove spaces and convert hex string to bytes
        var cleanHex = hexData.Replace(" ", "");
        var bytes = new byte[cleanHex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(cleanHex.Substring(i * 2, 2), 16);
        }

        // Parse binary data according to CanSat packet format
        var telemetry = new CanSatData
        {
            PacketCount = Math.Max(0, BitConverter.ToInt32(bytes, 4)), // bytes 4-7
            MissionTime = TimeSpan.FromSeconds(Math.Max(0, Math.Min(BitConverter.ToSingle(bytes, 8), 86400))), // bytes 8-11, limit to 24 hours
            Mode = bytes[12] == 0x46 ? "Flight" : "Simulation", // byte 12 (0x46 = 'F')
            Altitude = BitConverter.ToSingle(bytes, 16), // bytes 16-19
            Pressure = BitConverter.ToSingle(bytes, 20), // bytes 20-23
            Temperature = BitConverter.ToSingle(bytes, 24), // bytes 24-27
            Voltage = BitConverter.ToSingle(bytes, 28), // bytes 28-31
            GpsTime = TimeSpan.FromSeconds(Math.Max(0, Math.Min(BitConverter.ToSingle(bytes, 32), 86400))), // bytes 32-35, limit to 24 hours
            GpsLatitude = BitConverter.ToSingle(bytes, 36), // bytes 36-39
            GpsLongitude = BitConverter.ToSingle(bytes, 40), // bytes 40-43
            GpsAltitude = BitConverter.ToSingle(bytes, 44), // bytes 44-47
            GpsSats = Math.Max(0, BitConverter.ToInt32(bytes, 48)), // bytes 48-51
            TiltX = BitConverter.ToSingle(bytes, 52), // bytes 52-55
            TiltY = BitConverter.ToSingle(bytes, 56), // bytes 56-59
            TiltZ = BitConverter.ToSingle(bytes, 60), // bytes 60-63
            State = "Active", // Default state
        };

        return telemetry;
    }
}