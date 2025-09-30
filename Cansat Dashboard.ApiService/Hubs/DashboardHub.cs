using Cansat_Dashboard.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Cansat_Dashboard.ApiService.Hubs
{
    public class DashboardHub : Hub
    {
        // The "SendTelemetry" method has been removed.
        // The "ReceiveData" message is sent directly from the TelemetryService.
    }
}