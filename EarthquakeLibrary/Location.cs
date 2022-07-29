namespace EarthquakeLibrary.Core;

public class Location
{
    public Location(float latitude, float longitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
    public Location(){}
    /// <summary>
    /// 緯度
    /// </summary>
    public float Latitude { get; set; }
    /// <summary>
    /// 経度
    /// </summary>
    public float Longitude { get; set; }
}