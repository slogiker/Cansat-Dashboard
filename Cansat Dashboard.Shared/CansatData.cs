namespace Cansat_Dashboard.Shared
{
    public class CanSatData
    {
        // System
        public int PacketCount { get; set; }
        public long Timestamp { get; set; }       // ms since boot
        public float BatteryVoltage { get; set; }

        // Environment (BME680)
        public float Temperature { get; set; }    // °C
        public float Pressure { get; set; }       // hPa
        public float Humidity { get; set; }       // %
        public float VocIndex { get; set; }       // Air quality index

        // Light sensors
        public float UvIndex { get; set; }        // VEML6075
        public float Lux { get; set; }            // TSL2561

        // Magnetometer (BN0085 internal mag)
        public float MagX { get; set; }
        public float MagY { get; set; }
        public float MagZ { get; set; }

        // IMU (BN0085 accel + gyro)
        public float AccelX { get; set; }
        public float AccelY { get; set; }
        public float AccelZ { get; set; }
        public float GyroX { get; set; }
        public float GyroY { get; set; }
        public float GyroZ { get; set; }

        // Orientation (BN0085 Euler angles)
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        // GPS (BN-180)
        public float GpsLatitude { get; set; }
        public float GpsLongitude { get; set; }
        public float GpsAltitude { get; set; }
        public int GpsSatellites { get; set; }

        // Camera
        public int PhotoId { get; set; }          // Last saved photo ID

        // Commands / Status
        public string MissionState { get; set; } = string.Empty;
        public string CmdEcho { get; set; } = string.Empty;
    }
}
