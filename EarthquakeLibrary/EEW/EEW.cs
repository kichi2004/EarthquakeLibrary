using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using EarthquakeLibrary.Core;

namespace EarthquakeLibrary.EEW
{
    /// <summary>
    /// 緊急地震速報関連
    /// </summary>
    public static class Eew
    {

    }

    /// <summary>
    /// 緊急地震速報（新強震）
    /// </summary>
    public class EewData
    {

        /// <summary>
        /// 情報発表時刻
        /// </summary>
        public DateTime? ReportTime { get; set; }
        /// <summary>
        /// 震源コード
        /// </summary>
        public ushort? RegionCode { get; set; }
        /// <summary>
        /// 情報の時刻
        /// </summary>
        public DateTime? RequestTime { get; set; }
        /// <summary>
        /// 震央地名
        /// </summary>
        public string RegionName { get; set; }
        /// <summary>
        /// 震央座標
        /// </summary>
        public Location Location { get; set; }
        /// <summary>
        /// 深さ("km"まで入ります) 
        /// </summary>
        public string Depth { get; set; }
        /// <summary>
        /// 最大震度
        /// </summary>
        public Intensity Calcintensity { get; set; }
        /// <summary>
        /// 最終報か
        /// </summary>
        public bool? IsFinal { get; set; }
        /// <summary>
        /// 地震発生時刻
        /// </summary>
        public DateTime? OriginTime { get; set; }
        /// <summary>
        /// マグニチュード
        /// </summary>
        public float? Magunitude { get; set; }
        /// <summary>
        /// 報番号
        /// </summary>
        public byte? ReportNum { get; set; }
        /// <summary>
        /// 地震ID
        /// </summary>
        public string ReportId { get; set; }
    }



    /// <summary>
    /// 緊急地震速報データ
    /// </summary>
    internal class EewDataL10
    {
        /// <summary>
        /// 情報発表時刻
        /// </summary>
        public DateTime ReportTime { get; internal set; }
        /// <summary>
        /// 報番号
        /// </summary>
        public int ReoprtNum { get; internal set; }
        /// <summary>
        /// 最終報か
        /// </summary>
        public bool IsFinal { get; internal set; }
        /// <summary>
        /// 震源地
        /// </summary>
        public string Epicenter { get; internal set; }
        /// <summary>
        /// 震央コード
        /// </summary>
        public int RegionCode { get; internal set; }
        /// <summary>
        /// 深さ
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// マグニチュード
        /// </summary>
        public float Magnitude { get; set; }
        /// <summary>
        /// 最大震度
        /// </summary>
        public Intensity MaxIntensity { get; internal set; }
        /// <summary>
        /// 警報／予報／特別警報
        /// </summary>
        public WarningType Warning { get; internal set; }
        /// <summary>
        /// 震央座標
        /// </summary>
        public Location CenterLocation { get; internal set; }
        /// <summary>
        /// 訓練か
        /// </summary>
        public bool IsTraning { get; internal set; }
        /// <summary>
        /// 地震ID
        /// </summary>
        public long QuakeId { get; internal set; }
        /// <summary>
        /// 設定値の推定震度
        /// </summary>
        public Intensity EstimatedIntensity { get; internal set; }
        /// <summary>
        /// 都府県ごとの予想震度
        /// </summary>
        public IEnumerable<Area> Areas { get; internal set; }

        /// <summary>
        /// 推定地域
        /// </summary>
        public class Area
        {
            public string Prefecture { get; internal set; }
            public Intensity Intensity { get; internal set; }
        }
    }
    /// <summary>
    /// 緊急地震速報の種類
    /// </summary>
    public enum WarningType
    {
        /// <summary>
        /// なし／不明
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// 予報
        /// </summary>
        Foreacst = 0,
        /// <summary>
        /// 警報
        /// </summary>
        Warning = 1,
        Emergency = 2,
        /// <summary>
        /// 特別警報
        /// </summary>
        EmergencyWarning = Warning | Emergency
    }

    public class EewWatcher
    {
        /// <summary>
        /// EEWWatcherを初期化します。
        /// </summary>
        /// <param name="interval">取得間隔（秒）</param>
        public EewWatcher(ushort interval) : this()
            => this._timer.Interval = interval * 1000;
        /// <summary>
        /// EEWWatcherを初期化します。
        /// </summary>
        public EewWatcher()
        {
            this._timer = new System.Timers.Timer() { Interval = 1000 };
            this.SetTime();
            this._timer.Elapsed += this.Timer_Elapsed;
        }

        private System.Timers.Timer _timer;
        private int _c;
        private DateTime _time;
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (this._c == (int)this._timer.Interval / 5 * 18) this.SetTime();

            this._c++;
        }
        public void SetTime()
            => this._time = DateTime.ParseExact(new WebClient().DownloadString("http://ntp-a1.nict.go.jp/cgi-bin/time"), "ddd MMM dd HH:mm:ss yyyy JST\n", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);

    }
}
