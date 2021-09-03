using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable InconsistentNaming

namespace EarthquakeLibrary.Tsunami
{
    public static class Tsunami
    {
        /// <summary>
        /// 気象庁津波情報トップ
        /// </summary>
        public const string JmaTsunami = "http://www.jma.go.jp/jp/tsunami/";

        /// <summary>
        /// 気象庁サイトから津波到達予想情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static (IEnumerable<ForecastResult>, EpicenterInfo) GetForecastFromJma()
            => GetForecastFromJma(new Uri(JmaTsunami));

        /// <summary>
        /// 気象庁サイトから津波到達予想情報を取得します。
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public static (IEnumerable<ForecastResult>, EpicenterInfo) GetForecastFromJma(string url)
            => GetForecastFromJma(new Uri(url));

        /// <summary>
        /// 気象庁サイトから津波到達予想情報を取得します。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns></returns>
        public static (IEnumerable<ForecastResult>, EpicenterInfo) GetForecastFromJma(Uri uri) {
            var wc = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            //URLからダウンロード（ソースを取得）
            var source = wc.DownloadString(uri);
            //震源要素を正規表現で取得
            var match =
                new Regex("<td>地震の発生日時：&nbsp;(.*?)頃<br>震源地：&nbsp;(.*?)&nbsp;&nbsp;&nbsp;" +
                          "マグニチュード：&nbsp;(.{3}?)&nbsp;&nbsp;&nbsp;深さ：&nbsp;(.*?)</td>")
                    .Match(source);
            //EpicenterInfoクラスを作成
            var epicenter = match.Success
                ? new EpicenterInfo {
                    Time = DateTime.ParseExact(match.Groups[1].Value, "MM月dd日HH時mm分", null),
                    Epicenter = match.Groups[2].Value,
                    Depth = int.TryParse(match.Groups[4].Value.Replace("約", "").Replace("km", ""), out var d) ? d : 0,
                    Magnitude = match.Groups[3].Value.Contains("巨大") ? 0 : float.Parse(match.Groups[3].Value)
                }
                : null;

            //true:到達予想, false:大津波警報・津波警報・津波注意報
            var isE = true;
            //津波到達予想の場合の正規表現で検索
            var regex =
                @"<tr bgcolor='#\w+'><td.*?>＃?(.*?)</td><td class='tsunamiTime'.*?>＃?" +
                @"(.*?)</td><td.*?>(&nbsp;)*＃?(&nbsp;)*(.*?)</td></tr>";
            var matches = Regex.Matches(source, regex);
            //上記の正規表現でマッチしなかった場合(=到達予想ではない場合)
            if (matches.Count != 0) return (F(), epicenter);
            //大津波警報...の場合の正規表現で再検索
            regex = @"<tr bgcolor='#\w+'><td>＃?(.*?)</td><td>(.*?)</td></tr>";
            isE = false;
            matches = Regex.Matches(source, regex);

            return (F(), epicenter);

            IEnumerable<ForecastResult> F() {
                //マッチした各行
                foreach (Match m in matches) {
                    var res = new ForecastResult();
                    if (isE) {
                        //到達予想
                        res.ForecastPlace = m.Groups[1].Value;
                        var tmp = m.Groups[2].Value.Split('日').Last();
                        if (tmp.Contains("警報") || tmp.Contains("注意報")) {
                            res.Scale = WarningScale.Parse(tmp);
                            res.EstimatedHeight = ForecastHeights.Unknown;
                        } else {
                            res.EstimatedArrivalTime = tmp;
                        }

                        res.EstimatedHeight = ForecastHeight.Parse(m.Groups[5].Value);
                        res.Scale = res.EstimatedHeight.ToScale();
                    } else {
                        //not到達予想
                        res.ForecastPlace = m.Groups[1].Value;
                        res.Scale = WarningScale.Parse(m.Groups[2].Value);
                        res.EstimatedArrivalTime = null;
                        res.EstimatedHeight = ForecastHeights.Unknown;
                    }

                    yield return res;
                }
            }
        }

        /// <summary>
        /// tenki.jp津波情報
        /// </summary>
        public const string TenkiTsunami = "https://earthquake.tenki.jp/bousai/tsunami/observation/";

        /// <summary>
        /// tenki.jpから津波観測情報を取得します。
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IEnumerable<ObservationResult> GetObservationFromTenkiJp(Uri uri)
        {
            string html;
            using (var wc = new WebClient {Encoding = Encoding.UTF8})
                html = wc.DownloadString(uri);
            var matches = new Regex(
                @"<td class=""area-name"">(.+?)</td>\s+<td class=""point-name"">(.+?)</td>\s+<td class=""first-wave"">(.+?)</td>\s+<td class=""max-wave-time"">(.+?)</td>\s+ <td class=""max-wave-content"">(.+?)</td>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline).Matches(html);
            var ret = new List<ObservationResult>();
            foreach (Match m in matches)
            {
                var first = new First();
                if (m.Groups[3].Value.Contains("引き"))
                    first.Type = FirstType.Pull;
                else if (m.Groups[3].Value.Contains("押し"))
                    first.Type = FirstType.Push;
                else
                    first.Type = FirstType.Unknown;
                if (first.Type != FirstType.Unknown)
                {
                    DateTime.TryParseExact(m.Groups[3].Value.Split('(').First(), "M月d日 H時m分",
                        null, DateTimeStyles.None, out var dt);
                    first.Time = dt;
                }
                else first.Time = null;

                var obs = new ObservationResult
                {
                    Area = m.Groups[1].Value,
                    Point = m.Groups[2].Value,
                    First = first,
                    Max = m.Groups[4].Value == "---"
                        ? null
                        : (DateTime?) DateTime.ParseExact(m.Groups[4].Value, "M月d日 H時m分", null),
                    Rising = m.Groups[5].Value.Contains("上昇中")
                };
                switch (m.Groups[5].Value.Trim())
                {
                    case "微弱":
                        obs.Height = 0;
                        break;
                    case "観測中":
                        obs.Height = null;
                        break;
                    default:
                        var value = Regex.Match(
                            m.Groups[5].Value.Replace("\n", ""),
                            @"([0-9.]+?)m",
                            RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        obs.Height = float.Parse(value.Groups[1].Value);
                        break;
                }

                ret.Add(obs);
            }

            return ret;
        }

        /// <summary>
        /// tenki.jpから津波観測情報を取得します。
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IEnumerable<ObservationResult> GetObservationFromTenkiJp(string uri)
            => GetObservationFromTenkiJp(new Uri(uri));

        /// <summary>
        /// tenki.jpから津波観測情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ObservationResult> GetObservationFromTenkiJp()
            => GetObservationFromTenkiJp(new Uri(TenkiTsunami));

        /// <summary>
        /// tenki.jpから沖合観測情報を取得します。
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IEnumerable<ObservationResult> GetOffshoreFromTenkiJp(Uri uri) {
            string html;
            using (var wc = new WebClient { Encoding = Encoding.UTF8 })
                html = wc.DownloadString(uri);
            var str =
                new Regex("沖合の観測情報(?<text>.*?)津波の原因となった地震",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline)
                .Match(html).Groups["text"].Value;
            var matches = new Regex("<tr>\n    <td>(.*?)</td>\n    <td>(.*?)</td>\n    <td>(.*?)</td>\n    <td><span.*?>(.*?)</span></td>\n  </tr>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Matches(str);
            return Result();
            IEnumerable<ObservationResult> Result()
            {
                foreach (Match m in matches) {
                    var first = new First();
                    if (m.Groups[2].Value.Contains("引き"))
                        first.Type = FirstType.Pull;
                    else if (m.Groups[2].Value.Contains("押し"))
                        first.Type = FirstType.Push;
                    else
                        first.Type = FirstType.Unknown;
                    if (first.Type != FirstType.Unknown) {
                        DateTime.TryParseExact(m.Groups[2].Value.Split('(').First(), "M月d日 H時m分",
                            null, DateTimeStyles.None, out var dt);
                        first.Time = dt;
                    } else first.Time = null;
                    var obs = new ObservationResult
                    {
                        Point = m.Groups[1].Value.Split('(').First(),
                        First = first,
                        Max = m.Groups[3].Value == "---" ? null :
                        (DateTime?)DateTime.ParseExact(m.Groups[3].Value, "M月d日 H時m分", null) ,
                        Rising = m.Groups[4].Value.Contains("上昇中")
                    };
                    if (m.Groups[4].Value == "微弱") obs.Height = 0;
                    else if (m.Groups[4].Value == "観測中") obs.Height = null;
                    else obs.Height = float.Parse(m.Groups[4].Value.Split('(').First().Replace("<br />", "").Replace("m", ""));
                    yield return obs;
                }
            }
        }

        /// <summary>
        /// tenki.jpから沖合観測情報を取得します。
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IEnumerable<ObservationResult> GetOffshoreFromTenkiJp(string uri)
            => GetOffshoreFromTenkiJp(new Uri(uri));

        /// <summary>
        /// tenki.jpから沖合観測情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ObservationResult> GetOffshoreFromTenkiJp()
            => GetOffshoreFromTenkiJp(new Uri(TenkiTsunami));

    }

    /// <summary>
    /// 津波警報グレード
    /// </summary>
    public enum WarningScales
    {

        /// <summary>
        /// 不明
        /// </summary>
        [Description("不明")]
        Unknown,


        /// <summary>
        /// 津波注意報
        /// </summary>
        [Description("津波注意報")]
        TsunamiAdvisory,

        /// <summary>
        /// 津波警報
        /// </summary>
        [Description("津波警報")]
        TsunamiWarning,

        /// <summary>
        /// 大津波警報
        /// </summary>
        [Description("大津波警報")]
        MajorTsunamiWarning
    }

    /// <summary>
    /// 津波警報グレードの拡張メソッド
    /// </summary>
    public static class WarningScale
    {
        /// <summary>
        /// 文字列から津波警報スケールに変換します。
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns></returns>
        public static WarningScales Parse(string str) {
            switch (str) {
                case "津波注意報":
                    return WarningScales.TsunamiAdvisory;
                case "津波警報":
                case "津波の津波警報":
                    return WarningScales.TsunamiWarning;
                case "大津波警報":
                case "大津波の津波警報":
                        return WarningScales.MajorTsunamiWarning;
                default:
                        return WarningScales.Unknown;
            }
        }
        public static string ToString(this WarningScales scale) {
            switch (scale) {
                case WarningScales.TsunamiAdvisory:
                    return "津波注意報";
                case WarningScales.TsunamiWarning:
                    return "津波警報";
                case WarningScales.MajorTsunamiWarning:
                    return "大津波警報";
                default:
                    return "不明";
            }
        }
        public static WarningScales ToScale(this ForecastHeights height)
        {
            switch (height) {
                case ForecastHeights._1m:
                case ForecastHeights.Not_Notation:
                    return WarningScales.TsunamiAdvisory;
                case ForecastHeights._3m:
                case ForecastHeights.High:
                    return WarningScales.TsunamiWarning;
                case ForecastHeights._5m:
                case ForecastHeights._10m:
                case ForecastHeights.Over10m:
                case ForecastHeights.Huge:
                    return WarningScales.MajorTsunamiWarning;
                default:
                    return WarningScales.Unknown;
            }

        }
    }

    /// <summary>
    /// 津波到達予想
    /// </summary>
    public class ForecastResult
    {
        /// <summary>
        /// 津波警報グレード
        /// </summary>
        public WarningScales Scale { get; set; }

        /// <summary>
        /// 津波予報区
        /// </summary>
        public string ForecastPlace { get; set; }

        /// <summary>
        /// 津波到達予想時刻
        /// </summary>
        public string EstimatedArrivalTime { get; set; }

        /// <summary>
        /// 津波予想高さ
        /// </summary>
        public ForecastHeights EstimatedHeight { get; set; }

        public override bool Equals(object obj) {
            if (!(obj is ForecastResult res)) return false;
            return
                Scale == res.Scale &&
                ForecastPlace == res.ForecastPlace &&
                EstimatedArrivalTime == res.EstimatedArrivalTime &&
                EstimatedHeight == res.EstimatedHeight;
        }

        protected bool Equals(ForecastResult other)
        {
            return Scale == other.Scale &&
                   string.Equals(ForecastPlace, other.ForecastPlace) && 
                   string.Equals(EstimatedArrivalTime, other.EstimatedArrivalTime) &&
                   EstimatedHeight == other.EstimatedHeight;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Scale;
                hashCode = (hashCode * 397) ^ (ForecastPlace != null ? ForecastPlace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EstimatedArrivalTime != null ? EstimatedArrivalTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) EstimatedHeight;
                return hashCode;
            }
        }
    }

    /// <summary>
    /// 津波の予想高さ
    /// </summary>
    public enum ForecastHeights
    {
        /// <summary>
        /// 巨大
        /// </summary>
        [Description("巨大")]
        Huge,

        /// <summary>
        /// 10m超
        /// </summary>
        [Description("10m超")]
        Over10m,

        /// <summary>
        /// 10m
        /// </summary>
        [Description("10m")]
        _10m,

        /// <summary>
        /// 5m
        /// </summary>
        [Description("5m")]
        _5m,

        /// <summary>
        /// 高い
        /// </summary>
        [Description("高い")]
        High,

        /// <summary>
        /// 3m
        /// </summary>
        [Description("3m")]
        _3m,

        /// <summary>
        /// 表記しない
        /// </summary>
        [Description("")]
        Not_Notation,

        /// <summary>
        /// 1m
        /// </summary>
        [Description("1m")]
        _1m,

        /// <summary>
        /// 不明
        /// </summary>
        [Description("不明")]
        Unknown
    }

    /// <summary>
    /// 津波の予想高さの拡張メソッド
    /// </summary>
    public static class ForecastHeight
    {
        /// <summary>
        /// 文字列から高さに変換します。
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns></returns>
        public static ForecastHeights Parse(string str)
        {
            if (str == "1m" || str == "１ｍ" || str == "0.5 m")
                return ForecastHeights._1m;
            if (str == "3m" || str == "３ｍ" || str == "1 m" || str == "2 m")
                return ForecastHeights._3m;
            if (str == "5m" || str == "５ｍ" || str == "3 m" || str == "4 m")
                return ForecastHeights._5m;
            if (str == "10m" || str == "１０ｍ" || str == "6 m" || str == "8 m" || str == "10 m")
                return ForecastHeights._10m;
            if (str == "10m超" || str == "１０ｍ超" || str == "10m以上" || str == "10m超" || str == "10 m以上")
                return ForecastHeights.Over10m;
            if (str == "高い")
                return ForecastHeights.High;
            if (str == "巨大")
                return ForecastHeights.Huge;
            if (str == "")
                return ForecastHeights.Not_Notation;
            return ForecastHeights.Unknown;
        }


        /// <summary>
        /// 津波の予想高さを半角の文字列にします。
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string ToString(this ForecastHeights height)
        {
            switch (height) {
                case ForecastHeights._1m:
                    return "1m";
                case ForecastHeights._3m:
                    return "3m";
                case ForecastHeights._5m:
                    return "5m";
                case ForecastHeights._10m:
                    return "10m";
                case ForecastHeights.Over10m:
                    return "10m超";
                case ForecastHeights.High:
                    return "高い";
                case ForecastHeights.Huge:
                    return "巨大";
                case ForecastHeights.Not_Notation:
                    return "";
                default:
                    return "不明";
            }

        }

    }

    /// <summary>
    /// 震源情報
    /// </summary>
    public class EpicenterInfo
    {
        /// <summary>
        /// 発生時刻
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 震源情報
        /// </summary>
        public string Epicenter { get; set; }
        /// <summary>
        /// マグニチュード
        /// <para>8以上の巨大地震の場合は0</para>
        /// </summary>
        public float Magnitude { get; set; }
        /// <summary>
        /// 深さ
        /// <para>ごく浅い場合は0</para>
        /// </summary>
        public int Depth { get; set; }
    }


    /// <summary>
    /// 津波観測情報
    /// </summary>
    public class ObservationResult
    {
        /// <summary>
        /// 予報区
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 検潮所
        /// </summary>
        public string Point { get; set; }
        /// <summary>
        /// 第１波
        /// </summary>
        public First First { get; set; }
        /// <summary>
        /// 最大波
        /// </summary>
        public DateTime? Max { get; set; }
        /// <summary>
        /// 高さ
        /// </summary>
        public float? Height { get; set; }
        public bool Rising { get; set; }

        public override bool Equals(object obj) {
            if (!(obj is ObservationResult res)) return false;
            return
                Area == res.Area &&
                Point == res.Point &&
                First.Equals(res.First) &&
                Max == res.Max &&
                Height == res.Height &&
                Rising == res.Rising;
        }

        protected bool Equals(ObservationResult other)
        {
            return string.Equals(Area, other.Area) && string.Equals(Point, other.Point) && First.Equals(other.First) &&
                   Max.Equals(other.Max) && Height.Equals(other.Height) && Rising == other.Rising;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Area != null ? Area.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Point != null ? Point.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ First.GetHashCode();
                hashCode = (hashCode * 397) ^ Max.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                hashCode = (hashCode * 397) ^ Rising.GetHashCode();
                return hashCode;
            }
        }
    }

    public struct First
    {
        public FirstType Type { get; set; }
        public DateTime? Time { get; set; }
        public override bool Equals(object obj) {
            if (!(obj is First res)) return false;
            return
                Type == res.Type &&
                Time == res.Time;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
    public enum FirstType
    {
        /// <summary>
        /// 不明
        /// </summary>
        Unknown,
        /// <summary>
        /// 引き波
        /// </summary>
        Pull,
        /// <summary>
        /// 押し波
        /// </summary>
        Push
    }
}
