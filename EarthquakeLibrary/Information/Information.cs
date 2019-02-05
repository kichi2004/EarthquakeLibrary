// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EarthquakeLibrary.Core;
using HtmlAgilityPack;

namespace EarthquakeLibrary.Information
{
    public static class Information
    {
        /// <summary>
        /// 発生日時からYahoo!地震情報のURLを生成します。
        /// </summary>
        /// <param name="dateTime">発生日時</param>
        /// <returns></returns>
        public static string GenerateYahooUrl(DateTime dateTime)
            => $"https://typhoon.yahoo.co.jp/weather/earthquake/{dateTime:yyyyMMddHHmmss}.html";

        /// <summary>
        /// 発生日時からYahoo!地震情報のURLを生成します。
        /// </summary>
        /// <param name="dateTime">発生日時</param>
        /// <returns></returns>
        public static string GenerateYahooUrl(DateTime? dateTime) =>
            dateTime != null ? GenerateYahooUrl(dateTime.Value) : YahooUrl;

        /// <summary>
        /// Yahoo!天気・災害：地震 のトップページのURL
        /// </summary>
        public const string YahooUrl = "https://typhoon.yahoo.co.jp/weather/earthquake/";

        /// <summary>
        /// Yahoo!天気・災害：地震 のトップページのURL:URI
        /// </summary>
        public static Uri YahooUri = new Uri("https://typhoon.yahoo.co.jp/weather/earthquake/");

        /// <summary>
        /// Yahoo!天気・災害から最新の地震情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static NewEarthquakeInformation GetNewEarthquakeInformationFromYahoo()
            => GetNewEarthquakeInformationFromYahoo(YahooUri);

        /// <summary>
        /// Yahoo!天気・災害から最新地震情報を取得します。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns></returns>
        public static NewEarthquakeInformation GetNewEarthquakeInformationFromYahoo(Uri uri)
            => GetNewEarthquakeInformationFromYahooAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Yahoo!天気・災害から最新の地震情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static async Task<NewEarthquakeInformation> GetNewEarthquakeInformationFromYahooAsync()
            => await GetNewEarthquakeInformationFromYahooAsync(YahooUri);

        public static async Task<NewEarthquakeInformation> GetNewEarthquakeInformationFromYahooAsync(string url)
            => await GetNewEarthquakeInformationFromYahooAsync(new Uri(url));

        /// <summary>
        /// Yahoo!天気・災害から最新地震情報を取得します。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns></returns>
        public static async Task<NewEarthquakeInformation> GetNewEarthquakeInformationFromYahooAsync(Uri uri)
        {
            return await Task.Run( async () => {
                string source;
                try {
                    using (var wc = new WebClient() { Encoding = Encoding.UTF8, Proxy = null }) {
                        source = await wc.DownloadStringTaskAsync(uri);
                    }
                } catch (WebException) {
                    return null;
                }
                #region 正規表現
                var date_match =
                    new Regex(
                        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>発生時刻</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                //var date2_match =
                //    new Regex(
                //        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>情報発表時刻</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                //        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var region_match =
                    new Regex(
                        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>震源地</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small><a .*?>(.*?)</a>.*?</small></td>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var depth_match =
                    new Regex(
                        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>深さ</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var mag_match =
                    new Regex(
                        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>マグニチュード</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var int_match = new Regex("<td bgcolor=\"#ffffff\" width=10% align=center><small>震度(.*?)</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var info_match =
                    new Regex(
                        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>情報</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var latlon_match =
                    new Regex(
                        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>緯度/経度</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)/(.*?)</small></td>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var detail_url_match =
                    new Regex(
                        "<img src=\"(http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?)\" alt=\"全地点の震度\">",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var region_url_match =
                    new Regex(
                        "<img src=\"(http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?)\" alt=\"各地域の震度\">",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
                var match =
                    new Regex(
                        "<tr bgcolor=\"#ffffff\" valign=middle>.*?<td><a href=\"/weather/jp/earthquake(?<url>.*?)\">(?<time>.*?)</a></td>.*?<td align=center>(?<area>.*?)</td>.*?<td align=center>(?<mag>.*?)</td>.*?<td align=center>(?<sind>.*?)</td>.*?</tr>",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source).NextMatch();
                #endregion
                var lat = latlon_match.Groups[1].Value.Replace("度", "");
                var lon = latlon_match.Groups[2].Value.Replace("度", "");
                Location location = null;
                if (float.TryParse(lat.Substring(2), out var f))
                {
                    location = new Location {Latitude = f, Longitude = float.Parse(lon.Substring(2))};

                    if (!lon.Contains("東経")) location.Longitude *= -1;
                    if (!lat.Contains("北緯")) location.Longitude *= -1;
                }
                var rtn = new NewEarthquakeInformation(date_match.Groups[1].Value,
                    region_match.Groups[1].Value,
                    mag_match.Groups[1].Value, int_match.Groups[1].Value,
                    null, null, depth_match.Groups[1].Value, info_match.Groups[1].Value, detail_url_match.Groups[1].Value, region_url_match.Groups[1].Value) { 
                    Info_url = new Uri("https://typhoon.yahoo.co.jp/weather/earthquake/"),
                    Location = location,
                };

                var oldinfo = new List<OldEarthquakeInformation>();
                if (match.Success)
                    for (var i = 1; i < 9; i++) {
                        Console.WriteLine(match.Groups["time"].Value);
                        var info = new OldEarthquakeInformation {
                            Origin_time = DateTime.ParseExact(match.Groups["time"].Value, "yyyy年M月d日 H時m分ごろ",
                            DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault)
                            //Announced_time = DateTime.ParseExact(match.Groups["time2"].Value, "yyyy年M月d日 H時m分",
                            //DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault)
                        };
                        var epicenter = match.Groups["area"].Value;
                        info.Epicenter = epicenter == "---" ? null : epicenter;

                        var magnitude = match.Groups["mag"].Value;
                        info.Magnitude = magnitude == "---" ? (float?)null : float.Parse(magnitude);

                        var intensity = match.Groups["sind"].Value;
                        info.MaxIntensity = intensity == "---" ? Intensity.Unknown : Intensity.Parse(intensity);

                        info.Info_url = new Uri("https://typhoon.yahoo.co.jp/weather/jp/earthquake" + match.Groups["url"].Value);
                        oldinfo.Add(info);
                        if (i != 9)
                            match = match.NextMatch();
                    }
                rtn.Oldinfo = oldinfo.AsEnumerable();
                var doc = new HtmlDocument();
                doc.LoadHtml(source);

                var htmlNode = doc.DocumentNode.SelectSingleNode(@"//table[@class=""yjw_table""]")
                    .ChildNodes.Where((a, i) => i % 2 == 1);
                rtn.Shindo = htmlNode.Select(t => {
                    var info = new EarthquakeInformation.ShindoInformation();
                    var tag = t.ChildNodes;
                    info.Intensity = Intensity.Parse(tag[1].InnerText);
                    info.Place = tag[3].ChildNodes[1].ChildNodes
                    .Where((a, i) => i % 2 == 1)
                    .Select(a => a.ChildNodes)
                    .Select(a => new EarthquakeInformation.ShindoPlace {
                        Prefecture = a[1].InnerText.Replace("\n", ""),
                        Place = a[3].InnerText.Split('\n', ' ')
                            .Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))
                    });
                    return info;
                });
                return rtn;
            });
        }

        /// <summary>
        /// Yahoo!天気・災害から地震情報を取得します。
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public static EarthquakeInformation GetEarthquakeInformationFromYahoo(string url)
            => GetEarthquakeInformationFromYahoo(new Uri(url));

        /// <summary>
        /// Yahoo!天気・災害から地震情報を取得します。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns></returns>
        public static EarthquakeInformation GetEarthquakeInformationFromYahoo(Uri uri)
            => GetEarthquakeInformationFromYahooAsync(uri).GetAwaiter().GetResult();

        public static async Task<EarthquakeInformation> GetEarthquakeInformationFromYahooAsync(string url)
            => await GetEarthquakeInformationFromYahooAsync(new Uri(url));

        /// <summary>
        /// Yahoo!天気・災害から地震情報を取得します。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns></returns>
        public static async Task<EarthquakeInformation> GetEarthquakeInformationFromYahooAsync(Uri uri)
        {
            string source;
            try {
                using (var wc = new WebClient() { Encoding = Encoding.UTF8, Proxy = null}) {
                    source = await wc.DownloadStringTaskAsync(uri);
                }
            } catch (WebException) {
                return null;
            }
            #region 正規表現
            var date_match =
                new Regex(
                    "<td bgcolor=\"#eeeeee\" width=30% align=center><small>発生時刻</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            //var date2_match =
            //    new Regex(
            //        "<td bgcolor=\"#eeeeee\" width=30% align=center><small>情報発表時刻</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
            //        RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var region_match =
                new Regex(
                    "<td bgcolor=\"#eeeeee\" width=30% align=center><small>震源地</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small><a .*?>(.*?)</a>.*?</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var depth_match =
                new Regex(
                    "<td bgcolor=\"#eeeeee\" width=30% align=center><small>深さ</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var mag_match =
                new Regex(
                    "<td bgcolor=\"#eeeeee\" width=30% align=center><small>マグニチュード</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var int_match = new Regex("<td bgcolor=\"#ffffff\" width=10% align=center><small>震度(.*?)</small></td>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var info_match =
                new Regex(
                    "<td bgcolor=\"#eeeeee\" width=30% align=center><small>情報</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var latlon_match =
                new Regex(
                    "<td bgcolor=\"#eeeeee\" width=30% align=center><small>緯度/経度</small></td>.*?<td bgcolor=\"#ffffff\" width=70%><small>(.*?)/(.*?)</small></td>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var detail_url_match =
                new Regex(
                    "<img src=\"(http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?)\" alt=\"全地点の震度\">",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            var region_url_match =
                new Regex(
                    "<img src=\"(http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?)\" alt=\"各地域の震度\">",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source);
            #endregion
            var lat = latlon_match.Groups[1].Value;
            var lon = latlon_match.Groups[2].Value;
            Location location = null;
            if (float.TryParse(lat.Substring(2), out var f)) {
                location = new Location { Latitude = f, Longitude = float.Parse(lon.Substring(2)) };

                if (!lon.Contains("東経")) location.Longitude *= -1;
                if (!lat.Contains("北緯")) location.Longitude *= -1;
            }

            var rtn = new EarthquakeInformation(date_match.Groups[1].Value,
                region_match.Groups[1].Value,
                mag_match.Groups[1].Value, int_match.Groups[1].Value,
                null, null, depth_match.Groups[1].Value, info_match.Groups[1].Value, detail_url_match.Groups[1].Value,
                region_url_match.Groups[1].Value) {
                Info_url = new Uri("https://typhoon.yahoo.co.jp/weather/earthquake/"),
                Location = location,
            };

            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            var htmlNode = doc.DocumentNode.SelectSingleNode(@"//table[@class=""yjw_table""]")
                .ChildNodes.Where((a, i) => i % 2 == 1);
            rtn.Shindo = htmlNode.Select(t => {
                var info = new EarthquakeInformation.ShindoInformation();
                var tag = t.ChildNodes;
                info.Intensity = Intensity.Parse(tag[1].InnerText);
                info.Place = tag[3].ChildNodes[1].ChildNodes
                .Where((a, i) => i % 2 == 1)
                .Select(a => a.ChildNodes)
                .Select(a => new EarthquakeInformation.ShindoPlace {
                    Prefecture = a[1].InnerText.Replace("\n", ""),
                    Place = a[3].InnerText.Split('\n', ' ')
                        .Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))
                });
                return info;
            });
            return rtn;
        }

        public static IEnumerable<EarthquakeInformation.P2PShindoInformation> ToP2PShindoInformation(this IEnumerable<EarthquakeInformation.ShindoInformation> info)
        {
            return info
                .SelectMany(a => a.Place
                    .Select(b => new { a.Intensity, b.Prefecture, b.Place }))
                .GroupBy(a => a.Prefecture)
                .Select(a => new EarthquakeInformation.P2PShindoInformation {
                    Prefecture = a.Key, Place = a.Select(b =>
                      new EarthquakeInformation.P2PShindoPlace { Intensity = b.Intensity, Place = b.Place })
                });
        }

        public static IEnumerable<EarthquakeInformation.P2PShindoInformation> ToP2PShindoInformation(EarthquakeInformation info)
            => ToP2PShindoInformation(info.Shindo);
    }

    /// <inheritdoc />
    /// <summary>最新の地震情報</summary>
    public class NewEarthquakeInformation : EarthquakeInformation
    {
        public NewEarthquakeInformation() { }

        public NewEarthquakeInformation(string origin_time, string epicenter,
            string magnitude, string intensity, string Lat, string Lon, string depth, string message, string detailUrl, string regionUrl)
            : base(origin_time, epicenter,
            magnitude, intensity, Lat, Lon, depth, message, detailUrl, regionUrl)
        {
        }

        public IEnumerable<OldEarthquakeInformation> OldInformation { get; internal set; }

    }

    /// <inheritdoc />
    /// <summary>地震情報</summary>
    public class EarthquakeInformation : OldEarthquakeInformation
    {
        public EarthquakeInformation() { }

        public EarthquakeInformation(string origin_time, string epicenter, string magnitude,
            string intensity, string lat, string lon,
            string depth, string message, string detailUrl, string regionUrl) {
            this.Origin_time = DateTime.ParseExact(origin_time, "yyyy年M月d日 H時m分ごろ", DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.NoCurrentDateDefault);

            
            //this.Announced_time = DateTime.ParseExact(announced_time, "yyyy年M月d日 H時m分",
            //    DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault);

            this.Epicenter = epicenter == "---" || epicenter == "" ? null : epicenter;

            this.Magnitude = magnitude == "---" ? (float?) null : float.Parse(magnitude);

            this.MaxIntensity = intensity == "---" ? Intensity.Unknown : Intensity.Parse(intensity);

            this.Location = lat != null && lon != null ? new Location(float.Parse(lat), float.Parse(lon)) : null;

            switch (depth) {
                case "---":
                    this.Depth = null;
                    break;
                case "ごく浅い":
                    this.Depth = 0;
                    break;
                default:
                    this.Depth = short.Parse(depth.Replace("km", ""));
                    break;
            }

            this.DetailImageUrl = string.IsNullOrEmpty(detailUrl) ? null : new Uri(detailUrl);
            this.RegionImageUrl = string.IsNullOrEmpty(regionUrl) ? null : new Uri(regionUrl);

            var message2 = new List<MessageType>();
            if (message.Contains("津波警報等")) message2.Add(MessageType.TsunamiInformation);
            if (message.Contains("日本の沿岸では若干の海面変動")) message2.Add(MessageType.SeaLevelChange);
            if (message.Contains("この地震による津波の心配はありません。")) message2.Add(MessageType.NoTsunami);
            if (message.Contains("震源が海底の場合")) message2.Add(MessageType.TsunamiMayOccurIfEpicenterIsSea);
            if (message.Contains("今後の情報に注意")) message2.Add(MessageType.WarnToNextInfo);
            if (message.Contains("太平洋の広域")) message2.Add(MessageType.TsunamiMayOccurInWideAreaOfThePacificOcean);
            if (message.Contains("太平洋で津波発生の可能性")) {
                message2.Add(message.Contains("北西大西洋")
                    ? MessageType.TsunamiMayOccurInNorthWestPacificOcean
                    : MessageType.TsunamiMayOccurInThePacificOcean);
            }

            if (message.Contains("インド洋の広域")) message2.Add(MessageType.TsunamiMayOccurInWideAreaOfIndianTheOcean);
            if (message.Contains("インド洋")) message2.Add(MessageType.TsunamiMayOccurInTheIndianOcean);
            if (message.Contains("震源の近傍で津波発生")) message2.Add(MessageType.TsunamiMayOccurNearTheEpicenter);
            if (message.Contains("震源の近傍で小さな津波"))
                message2.Add(MessageType.LittleTsunamiMayOccurNearTheEpicenter_ButNoDamage);
            if (message.Contains("一般的"))
                message2.Add(MessageType.TsunamiMayOccurIfAboutThisScaleEarthquakeOccursInShallowSeaInGeneral);
            if (message.Contains("現在調査中")) message2.Add(MessageType.InvestigatingForTsunamiInJapan);
            if (message.Contains("日本への津波の影響は")) message2.Add(MessageType.NoTsunamiInJapan);
            if (message.Contains("緊急地震速報")) {
                var b = false;
                if (message.Contains("最大震度は２")) message2.Add(MessageType.EEW_Int2);
                else if (message.Contains("最大震度は１")) message2.Add(MessageType.EEW_Int1);
                else if (message.Contains("震度１以上は")) message2.Add(MessageType.EEW_NoShake);
                else b = true;
                if (message.Contains("強い揺れは観測")) message2.Add(MessageType.EEW_NoStrongShake);
                else if (b) message2.Add(MessageType.EEW);
            }

            this.Message = message2.ToArray();
            this.Info_url = null;
        }

        /// <summary>
        /// 座標
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// 深さ
        /// </summary>
        public short? Depth { get; set; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public MessageType[] Message { get; set; }

        /// <summary>
        /// 過去情報
        /// </summary>
        public IEnumerable<OldEarthquakeInformation> Oldinfo { get; set; }

        /// <summary>
        /// 画像のURL
        /// </summary>
        [Obsolete("DetailImageUrlまたはRegionImageUrlを使用してください。", true)]
        public Uri Image_Url { get; set; }

        /// <summary>
        /// 全地点の震度の画像URL
        /// </summary>
        public Uri DetailImageUrl { get; set; }

        /// <summary>
        /// 各地域の震度の画像URL
        /// </summary>
        public Uri RegionImageUrl { get; set; }

        /// <summary>
        /// 震度情報
        /// </summary>
        public IEnumerable<ShindoInformation> Shindo { get; set; }

        /// <summary>
        /// 情報の種類
        /// </summary>
        public InformationType InformationType
        {
            get {
                //震度速報
                if (this.Epicenter == null)
                    return InformationType.SesimicInfo;
                if (this.Shindo.Any()) {
                    return DetailImageUrl == null ? InformationType.EpicenterInfo : InformationType.EarthquakeInfo;
                }
                //震度不明
                return InformationType.UnknownSesimic;
            }
        }
            //(InformationType)
              //      ( this.Epicenter == null ? 0 :
                //        this.Shindo.Any() ? 
                  //          this.Image_Url == null ? 1 : 2 : 3 );

        public class ShindoInformation
        {
            public ShindoInformation() { }
            public ShindoInformation(string shindo, IEnumerable<ShindoPlace> place)
            {
                this.Intensity = Intensity.Parse(shindo);
                this.Place = place;
            }

            /// <summary>
            /// 震度
            /// </summary>
            public Intensity Intensity { get; set; }

            /// <summary>
            /// 観測地点
            /// </summary>
            public IEnumerable<ShindoPlace> Place { get; set; }

            public static bool operator ==(ShindoInformation a, ShindoInformation b)
                => a.Intensity == b.Intensity && a.Place.SequenceEqual(b.Place);

            public static bool operator !=(ShindoInformation a, ShindoInformation b)
                => !( a == b );
            public override int GetHashCode() => throw new NotImplementedException();
            public override bool Equals(object obj)
            {
                if (!( obj is ShindoInformation b )) return false;
                return this == b;
            }

        }
        public class ShindoPlace
        {
            public ShindoPlace() { }
            public ShindoPlace(string prefcture, IEnumerable<string> place)
            {
                this.Prefecture = prefcture;
                this.Place = place;
            }
            /// <summary>
            /// 都道府県
            /// </summary>
            public string Prefecture { get; set; }
            /// <summary>
            /// 観測 市区町村
            /// </summary>
            public IEnumerable<string> Place { get; set; }

            public static bool operator ==(ShindoPlace a, ShindoPlace b)
                => a is null
                    ? b is null
                    : !(b is null) && a.Prefecture == b.Prefecture && a.Place.SequenceEqual(b.Place);
            public static bool operator !=(ShindoPlace a, ShindoPlace b)
                => !( a == b );
            public override bool Equals(object obj)
            {
                if (!( obj is ShindoPlace b )) return false;
                return this == b;
            }
            /// <summary>
            /// このメソッドは使わないでください。
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode() => throw new NotImplementedException();

        }

        public class P2PShindoInformation
        {
            /// <summary>
            /// 都道府県
            /// </summary>
            public string Prefecture { get; set; }

            public IEnumerable<P2PShindoPlace> Place { get; set; }

        }
        public class P2PShindoPlace
        {
            /// <summary>
            /// 震度
            /// </summary>
            public Intensity Intensity { get; set; }

            /// <summary>
            /// 観測 市区町村
            /// </summary>
            public IEnumerable<string> Place { get; set; }
        }
    }

    /// <summary>
    /// 地震情報の種類
    /// </summary>
    public enum InformationType
    {
        /// <summary>
        /// 震度速報
        /// </summary>
        [Description("震度速報")]
        SesimicInfo,

        /// <summary>
        /// 震源速報
        /// </summary>
        [Description("震源速報")]
        EpicenterInfo,

        /// <summary>
        /// 地震情報
        /// </summary>
        [Description("各地の震度情報")]
        EarthquakeInfo,

        /// <summary>
        /// 震度不明
        /// </summary>
        [Description("震度不明")]
        UnknownSesimic,

        /// <summary>
        /// その他
        /// </summary>
        [Description("その他")]
        Other
    }

    /// <summary>
    /// 過去の地震情報
    /// </summary>
    public class OldEarthquakeInformation
    {
        public OldEarthquakeInformation() { }
        /// <summary>
        /// 過去の地震情報を発生時刻・発表時刻・震源地・マグニチュード・震度・URLで初期化します。
        /// </summary>
        /// <param name="origin_time">発生時刻</param>
        /// <param name="epicenter">震源地</param>
        /// <param name="magnitude">マグニチュード</param>
        /// <param name="intensity">震度</param>
        /// <param name="uri">URL</param>
        public OldEarthquakeInformation(string origin_time, string epicenter, string magnitude, string intensity, string uri)
        {
            this.Origin_time = DateTime.ParseExact(origin_time, "yyyy年M月d日 H時m分ごろ", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault);

            //this.Announced_time = DateTime.ParseExact(announced_time, "yyyy年M月d日 H時m分", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault);

            this.Epicenter = epicenter == "---" ? null : epicenter;

            this.Magnitude = magnitude == "---" ? (float?) null : float.Parse(magnitude);

            this.MaxIntensity = intensity == "---" ? Intensity.Unknown : Intensity.Parse(intensity);

            this.Info_url = new Uri(uri, UriKind.Absolute);
        }

        /// <summary>
        /// 発生時刻
        /// </summary>
        public DateTime Origin_time { get; set; }

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

    /// <summary>
    /// 情報の種類
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 津波警報等（大津波警報・津波警報あるいは津波注意報）を発表中です。
        /// </summary>
        [Description("津波警報等（大津波警報・津波警報あるいは津波注意報）を発表中です。")]
        TsunamiInformation,
        /// <summary>
        /// この地震により、日本の沿岸では若干の海面変動があるかもしれませんが、被害の心配はありません。
        /// </summary>
        [Description("この地震により、日本の沿岸では若干の海面変動があるかもしれませんが、被害の心配はありません。")]
        SeaLevelChange,
        /// <summary>
        /// この地震による津波の心配はありません。
        /// </summary>
        [Description("この地震による津波の心配はありません。")]
        NoTsunami,
        /// <summary>
        /// 震源が海底の場合、津波が発生するおそれがあります。
        /// </summary>
        [Description("震源が海底の場合、津波が発生するおそれがあります。")]
        TsunamiMayOccurIfEpicenterIsSea,
        /// <summary>
        /// 今後の情報に注意してください。
        /// </summary>
        [Description("今後の情報に注意してください。")]
        WarnToNextInfo,
        /// <summary>
        /// 太平洋の広域に津波発生の可能性があります。
        /// </summary>
        [Description("太平洋の広域に津波発生の可能性があります。")]
        TsunamiMayOccurInWideAreaOfThePacificOcean,
        /// <summary>
        /// 太平洋で津波発生の可能性があります。
        /// </summary>
        [Description("太平洋で津波発生の可能性があります。")]
        TsunamiMayOccurInThePacificOcean,
        /// <summary>
        /// 北西太平洋で津波発生の可能性があります。
        /// </summary>
        [Description("北西太平洋で津波発生の可能性があります。")]
        TsunamiMayOccurInNorthWestPacificOcean,
        /// <summary>
        /// インド洋の広域に津波発生の可能性があります。
        /// </summary>
        [Description("インド洋の広域に津波発生の可能性があります。")]
        TsunamiMayOccurInWideAreaOfIndianTheOcean,
        /// <summary>
        /// インド洋で津波発生の可能性があります。
        /// </summary>
        [Description("インド洋で津波発生の可能性があります。")]
        TsunamiMayOccurInTheIndianOcean,
        /// <summary>
        /// 震源の近傍で津波発生の可能性があります。
        /// </summary>
        [Description("震源の近傍で津波発生の可能性があります。")]
        TsunamiMayOccurNearTheEpicenter,
        /// <summary>
        /// 震源の近傍で小さな津波発生の可能性がありますが、被害をもたらす津波の心配はありません。
        /// </summary>
        [Description("震源の近傍で小さな津波発生の可能性がありますが、被害をもたらす津波の心配はありません。")]
        LittleTsunamiMayOccurNearTheEpicenter_ButNoDamage,
        /// <summary>
        /// 一般的に、この規模の地震が海域の浅い領域で発生すると、津波が発生することがあります。
        /// </summary>
        [Description("一般的に、この規模の地震が海域の浅い領域で発生すると、津波が発生することがあります。")]
        TsunamiMayOccurIfAboutThisScaleEarthquakeOccursInShallowSeaInGeneral,
        /// <summary>
        /// 日本への津波の有無については現在調査中です。
        /// </summary>
        [Description("日本への津波の有無については現在調査中です。")]
        InvestigatingForTsunamiInJapan,
        /// <summary>
        /// この地震による日本への津波の影響はありません。
        /// </summary>
        [Description("この地震による日本への津波の影響はありません。")]
        NoTsunamiInJapan,
        /// <summary>
        /// この地震について、緊急地震速報を発表しています。
        /// </summary>
        [Description("この地震について、緊急地震速報を発表しています。")]
        EEW,
        /// <summary>
        /// この地震について、緊急地震速報を発表しています。この地震の最大震度は２でした。
        /// </summary>
        [Description("この地震について、緊急地震速報を発表しています。この地震の最大震度は２でした。")]
        EEW_Int2,
        /// <summary>
        /// この地震について、緊急地震速報を発表しています。この地震の最大震度は１でした。
        /// </summary>
        [Description("この地震について、緊急地震速報を発表しています。この地震の最大震度は１でした。")]
        EEW_Int1,
        /// <summary>
        /// この地震について、緊急地震速報を発表しています。この地震で震度１以上は観測されていません。
        /// </summary>
        [Description("この地震について、緊急地震速報を発表しています。この地震で震度１以上は観測されていません。")]
        EEW_NoShake,
        /// <summary>
        /// この地震で緊急地震速報を発表しましたが、強い揺れは観測されませんでした。
        /// </summary>
        [Description("この地震で緊急地震速報を発表しましたが、強い揺れは観測されませんでした。")]
        EEW_NoStrongShake

    }
}