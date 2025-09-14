namespace Cansat_Dashboard.ApiService.Data;

public class CanSatData
{
    public float Temperature { get; set; }
    public float Pressure { get; set; }
    public float Humidity { get; set; }
    public int SatelliteCount { get; set; }
    public int UvIndex { get; set; }
    public int Lux { get; set; }
    public int VocIndex { get; set; }
    public string PictureStatus { get; set; } = "OK";
    public Vector3 MagneticField { get; set; } = new();
    public Vector3 Acceleration { get; set; } = new();
    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float Yaw { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Altitude { get; set; }
    public int GpsAltitude { get; set; }
    public string MissionTime { get; set; } = "00:00:00";
    public string Battery { get; set; } = "---%";
}

public class Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
