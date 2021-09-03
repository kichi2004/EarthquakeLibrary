using System.Collections.Generic;

namespace EarthquakeLibrary.Information
{
    /// <inheritdoc />
    /// <summary>最新の地震情報</summary>
    public class NewEarthquakeInformation : EarthquakeInformation
    {
        /// <summary>
        /// 最新の地震情報クラスのインスタンスを初期化します。
        /// </summary>
        public NewEarthquakeInformation()
        {
        }

        /// <summary>
        /// 地震の情報を利用して、最新の地震情報クラスのインスタンスを初期化します。
        /// </summary>
        public NewEarthquakeInformation(string origin_time, string epicenter,
            string magnitude, string intensity, string Lat, string Lon, string depth, string message, string detailUrl,
            string regionUrl)
            : base(origin_time, epicenter,
                magnitude, intensity, Lat, Lon, depth, message, detailUrl, regionUrl)
        {
        }

        /// <summary>
        /// 地震情報の履歴
        /// </summary>
        public IEnumerable<OldEarthquakeInformation> InformationHistory { get; internal set; }

    }
}