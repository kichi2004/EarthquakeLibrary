using System.ComponentModel;

namespace EarthquakeLibrary.Information
{
    /// <summary>
    /// 地震情報の種類
    /// </summary>
    public enum InformationType
    {
        /// <summary>
        /// 震度速報
        /// </summary>
        [Description("震度速報")] SesimicInfo,

        /// <summary>
        /// 震源速報
        /// </summary>
        [Description("震源速報")] EpicenterInfo,

        /// <summary>
        /// 地震情報
        /// </summary>
        [Description("各地の震度情報")] EarthquakeInfo,

        /// <summary>
        /// 震度不明
        /// </summary>
        [Description("震度不明")] UnknownSesimic,

        /// <summary>
        /// その他
        /// </summary>
        [Description("その他")] Other
    }
}