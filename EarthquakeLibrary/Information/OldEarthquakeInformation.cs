using System;
using System.Globalization;

namespace EarthquakeLibrary.Information;

/// <summary>
/// 過去の地震情報
/// </summary>
public class OldEarthquakeInformation
{
    /// <summary></summary>
    protected OldEarthquakeInformation() {}
    /// <summary>
    /// 過去の地震情報を発生時刻・発表時刻・震源地・マグニチュード・震度・URLで初期化します。
    /// </summary>
    /// <param name="origin_time">発生時刻</param>
    /// <param name="epicenter">震源地</param>
    /// <param name="magnitude">マグニチュード</param>
    /// <param name="intensity">震度</param>
    /// <param name="uri">URL</param>
    public OldEarthquakeInformation(string origin_time, string epicenter, string magnitude, string intensity,
        string uri)
    {
        OriginTime = DateTime.ParseExact(origin_time, "yyyy年M月d日 H時m分ごろ", DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.NoCurrentDateDefault);

        //this.Announced_time = DateTime.ParseExact(announced_time, "yyyy年M月d日 H時m分", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault);

        Epicenter = epicenter == "---" ? null : epicenter;

        Magnitude = magnitude == "---" ? (float?) null : float.Parse(magnitude);

        MaxIntensity = intensity == "---" ? Intensity.Unknown : Intensity.Parse(intensity);

        Info_url = new Uri(uri, UriKind.Absolute);
    }

    /// <summary>
    /// 発生時刻
    /// </summary>
    [Obsolete("OriginTimeを使用してください。")]
    public DateTime Origin_time => OriginTime ?? throw new NullReferenceException();

    /// <summary>
    /// 発生時刻
    /// </summary>
    public DateTime? OriginTime { get; set; }

    /// <summary>
    /// 震源地
    /// </summary>
    public string Epicenter { get; set; }

    /// <summary>
    /// マグニチュード
    /// </summary>
    public float? Magnitude { get; set; }

    /// <summary>
    /// 最大震度
    /// </summary>
    public Intensity MaxIntensity { get; set; }

    /// <summary>
    /// 情報のURL
    /// </summary>
    public Uri Info_url { get; set; }
}