using SQLite;

namespace LocationHeatMap.Models;

public class LocationPoint
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public DateTime TimestampUtc { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double AccuracyMeters { get; set; }
}