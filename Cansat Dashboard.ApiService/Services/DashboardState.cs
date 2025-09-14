// File: Services/DashboardState.cs
namespace Cansat_Dashboard.Web.Services
{
    public class DashboardState
    {
        public string MissionTime { get; private set; } = "00:00:00";
        public string Battery { get; private set; } = "---%";

        public event Action? OnChange;

        public void SetHeaderData(string missionTime, string battery)
        {
            MissionTime = missionTime;
            Battery = battery;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}