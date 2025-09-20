using Cansat_Dashboard.ApiService;

namespace Cansat_Dashboard.Web.Components;

public class DashboardState
{
    public event Action? OnChange;
    
    private CanSatData? _currentData;
    
    public CanSatData? CurrentData
    {
        get => _currentData;
        set
        {
            _currentData = value;
            OnChange?.Invoke();
        }
    }
    
    public string MissionTime => _currentData?.MissionTime.ToString(@"hh\:mm\:ss") ?? "00:00:00";
    public string Battery => _currentData?.Voltage.ToString("F2") + "V" ?? "0.00V";
    public string State => _currentData?.State ?? "Unknown";
    public string Mode => _currentData?.Mode ?? "Unknown";
}
