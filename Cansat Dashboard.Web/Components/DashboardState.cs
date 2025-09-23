using Cansat_Dashboard.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cansat_Dashboard.Web.Components
{
    public class DashboardState : IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private HubConnection? hubConnection;
        private Timer? mockDataTimer;
        private readonly Random _random = new();

        public List<CanSatData> Data { get; private set; } = new();
        public bool IsConnected { get; private set; } = false;

        public event Action? OnChange;

        public DashboardState(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Init()
        {
            var apiServiceUrl = _configuration["services:apiservice:http:0"] ?? "http://localhost:5106";

            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{apiServiceUrl}/dashboardhub")
                .Build();

            hubConnection.On<CanSatData>("ReceiveData", (data) =>
            {
                Data.Add(data);
                NotifyStateChanged();
            });

            try
            {
                await hubConnection.StartAsync();
                IsConnected = true;
            }
            catch (Exception)
            {
                IsConnected = false;
                StartMockDataGenerator();
            }
            NotifyStateChanged();
        }

        private void StartMockDataGenerator()
        {
            mockDataTimer = new Timer(_ =>
            {
                var mockData = new CanSatData
                {
                    PacketCount = Data.Count + 1,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    BatteryVoltage = 4.1f + (float)(_random.NextDouble() * 0.2 - 0.1),
                    Temperature = 25 + (float)(_random.NextDouble() * 2 - 1),
                    Pressure = 1013 + (float)(_random.NextDouble() * 10 - 5),
                    Humidity = 45 + (float)(_random.NextDouble() * 10 - 5),
                    GpsLatitude = 46.0569f + (float)(_random.NextDouble() * 0.001 - 0.0005),
                    GpsLongitude = 14.5058f + (float)(_random.NextDouble() * 0.001 - 0.0005),
                    GpsAltitude = 300 + (float)(_random.NextDouble() * 20 - 10),
                    GpsSatellites = _random.Next(8, 13),
                    Pitch = (float)(_random.NextDouble() * 2 - 1),
                    Roll = (float)(_random.NextDouble() * 2 - 1),
                    Yaw = (float)(_random.NextDouble() * 360),
                    MissionState = "STANDBY"
                };
                Data.Add(mockData);
                NotifyStateChanged();
            }, null, 0, 1000);
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async ValueTask DisposeAsync()
        {
            mockDataTimer?.Dispose();
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}