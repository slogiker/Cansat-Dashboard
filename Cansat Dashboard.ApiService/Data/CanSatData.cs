namespace Cansat_Dashboard.ApiService;

public class CanSatData
{
    public int PacketCount { get; set; }
    public TimeSpan MissionTime { get; set; }
    public string Mode { get; set; } = string.Empty;
    public bool IsSimulation => Mode == "S";
    public float Altitude { get; set; }
    public float Pressure { get; set; }
    public float Temperature { get; set; }
    public float Voltage { get; set; }
    public TimeSpan GpsTime { get; set; }
    public float GpsLatitude { get; set; }
    public float GpsLongitude { get; set; }
    public float GpsAltitude { get; set; }
    public int GpsSats { get; set; }
    public float TiltX { get; set; }
    public float TiltY { get; set; }
    public float TiltZ { get; set; }
    public string State { get; set; } = string.Empty;
}