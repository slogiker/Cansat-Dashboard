using Cansat_Dashboard.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Cansat_Dashboard.ApiService.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task SendTelemetry(CanSatData data)
        {
            await Clients.All.SendAsync("ReceiveTelemetry", data);
        }
    }
}