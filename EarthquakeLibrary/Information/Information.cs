// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EarthquakeLibrary.Core;
using HtmlAgilityPack;

namespace EarthquakeLibrary.Information
{
    /// <summary>
    /// 地震情報の取得操作など
    /// </summary>
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
            => GetNewEarthquakeInformationFromYahooAsync(uri).GetAwaiter().GetResult();

        /// <summary>
        /// Yahoo!天気・災害から最新の地震情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static async Task<NewEarthquakeInformation> GetNewEarthquakeInformationFromYahooAsync()
            => await GetNewEarthquakeInformationFromYahooAsync(YahooUri);

        /// <summary>
        /// Yahoo!天気・災害から最新の地震情報を取得します。
        /// </summary>
        /// <returns></returns>
        public static async Task<NewEarthquakeInformation> GetNewEarthquakeInformationFromYahooAsync(string url)
            => await GetNewEarthquakeInformationFromYahooAsync(new Uri(url));

        /// <summary>
        /// Yahoo!天気・災害から最新地震情報を取得します。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns></returns>
        public static async Task<NewEarthquakeInformation> GetNewEarthquakeInformationFromYahooAsync(Uri uri)
        {
            string source;
            try
            {
                source = await LibCore.DownloadString(uri);
            }
            catch (WebException)
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(source);
            var rows = doc.DocumentNode.SelectNodes("//*[@id=\"eqinfdtl\"]/table[1]/tr");
            var datas = new Dictionary<string, string>();
            foreach (var node in rows) {
                var title = node.SelectSingleNode("td[1]/small");
                var data = node.SelectSingleNode("td[2]/small");
                datas.Add(title.InnerText, data.InnerText.Trim());
            }

            var regionUrl = doc.DocumentNode.SelectSingleNode("//img[@alt=\"各地域の震度\"]");
            var detailUrl = doc.DocumentNode.SelectSingleNode("//img[@alt=\"全地点の震度\"]");
            var ints = doc.DocumentNode.SelectSingleNode("//*[@id=\"eqinfdtl\"]/table[2]/tr[1]/td[1]/small");

            var latlon = datas["緯度/経度"].Replace("度", "").Split('/');
            var (lat, lon) = (latlon[0], latlon[1]);
            Location location = null;
            if (float.TryParse(lat.Substring(2), out var f))
            {
                location = new Location {Latitude = f, Longitude = float.Parse(lon.Substring(2))};

                if (!lon.Contains("東経")) location.Longitude *= -1;
                if (!lat.Contains("北緯")) location.Longitude *= -1;
            }

            var rtn = new NewEarthquakeInformation(
                datas["発生時刻"], datas["震源地"], datas["マグニチュード"],
                ints?.InnerText, null, null, datas["深さ"], 
                datas["情報"], detailUrl?.Attributes["src"].Value, regionUrl?.Attributes["src"].Value)
            {
                Info_url = new Uri("https://typhoon.yahoo.co.jp/weather/earthquake/"),
                Location = location
            };

            var match = new Regex(
                "<tr bgcolor=\"#ffffff\" valign=middle>.*?<td>.*?<a href=\"/weather/jp/earthquake(?<url>.*?)\">(?<time>.*?)</a>.*?</td>.*?<td align=center>(?<area>.*?)</td>.*?<td align=center>(?<mag>.*?)</td>.*?<td align=center>(?<sind>.*?)</td>.*?</tr>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(source).NextMatch();
            var oldinfo = new List<OldEarthquakeInformation>();
            if (match.Success)
                for (var i = 1; i < 9; i++)
                {
                    var epicenter = match.Groups["area"].Value;
                    var magnitude = match.Groups["mag"].Value;
                    var intensity = match.Groups["sind"].Value;

                    oldinfo.Add(new OldEarthquakeInformation(match.Groups["time"].Value, epicenter, magnitude,
                        intensity, "https://typhoon.yahoo.co.jp/weather/jp/earthquake" +
                                   match.Groups["url"].Value));
                    match = match.NextMatch();
                }

            rtn.Oldinfo = oldinfo.AsEnumerable();

            var htmlNode = doc.DocumentNode.SelectSingleNode(@"//table[@class=""yjw_table""]")
                .ChildNodes.Where((a, i) => i % 2 == 1);
            rtn.Shindo = htmlNode.Select(t =>
            {
                var tag = t.ChildNodes;
                var info = new EarthquakeInformation.ShindoInformation(tag[1].InnerText.Trim(),
                    tag[3].ChildNodes[1].ChildNodes
                        .Where((a, i) => i % 2 == 1)
                        .Select(a => a.ChildNodes)
                        .Select(a => new EarthquakeInformation.ShindoPlace
                        (
                            a[1].InnerText.Trim(),
                            a[3].InnerText.Split('\n', ' ').Select(x => x.Trim())
                                .Where(x => !string.IsNullOrWhiteSpace(x) && !x.Contains('-'))
                        )));
                return info;
            });
            return rtn;
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

        /// <summary>
        /// Yahoo!天気・災害から地震情報を取得します。
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
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
            try
            {
                source = await LibCore.DownloadString(uri);
            }
            catch (WebException)
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(source);
            var rows = doc.DocumentNode.SelectNodes("//*[@id=\"eqinfdtl\"]/table[1]/tr");
            var datas = new Dictionary<string, string>();
            foreach (var node in rows) {
                var title = node.SelectSingleNode("td[1]/small");
                var data = node.SelectSingleNode("td[2]/small");
                datas.Add(title.InnerText, data.InnerText.Trim());
            }

            var regionUrl = doc.DocumentNode.SelectSingleNode("//img[@alt=\"各地域の震度\"]");
            var detailUrl = doc.DocumentNode.SelectSingleNode("//img[@alt=\"全地点の震度\"]");
            var ints = doc.DocumentNode.SelectSingleNode("//*[@id=\"eqinfdtl\"]/table[2]/tr[1]/td[1]/small");
            
            var latlon = datas["緯度/経度"].Replace("度", "").Split('/');
            var (lat, lon) = (latlon[0], latlon[1]);
            Location location = null;
            if (float.TryParse(lat.Substring(2), out var f))
            {
                location = new Location {Latitude = f, Longitude = float.Parse(lon.Substring(2))};

                if (!lon.Contains("東経")) location.Longitude *= -1;
                if (!lat.Contains("北緯")) location.Longitude *= -1;
            }

            var rtn = new EarthquakeInformation(
                datas["発生時刻"], datas["震源地"], datas["マグニチュード"],
                ints?.InnerText, null, null, datas["深さ"], 
                datas["情報"], detailUrl?.Attributes["src"].Value, regionUrl?.Attributes["src"].Value)
            {
                Info_url = new Uri("https://typhoon.yahoo.co.jp/weather/earthquake/"),
                Location = location
            };


            var htmlNode = doc.DocumentNode.SelectSingleNode(@"//table[@class=""yjw_table""]")
                .ChildNodes.Where((a, i) => i % 2 == 1);
            rtn.Shindo = htmlNode.Select(t =>
            {
                var tag = t.ChildNodes;
                return new EarthquakeInformation.ShindoInformation(tag[1].InnerText.Trim(),
                    tag[3].ChildNodes[1]
                        .ChildNodes
                        .Where((a, i) => i % 2 == 1)
                        .Select(a => a.ChildNodes)
                        .Select(a => new EarthquakeInformation.ShindoPlace
                        (
                            a[1].InnerText.Trim(),
                            a[3].InnerText.Split('\n', ' ')
                                .Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))
                        )));
            });
            return rtn;
        }

        /// <summary>
        /// <see cref="EarthquakeInformation.ShindoInformation"/> の震度情報を、P2P地震情報の形式に変換します。
        /// </summary>
        /// <param name="info">震度情報</param>
        /// <returns>P2P地震情報の形式に変換された震度情報</returns>
        public static IEnumerable<EarthquakeInformation.P2PShindoInformation> ToP2PShindoInformation(
            this IEnumerable<EarthquakeInformation.ShindoInformation> info)
            => info.SelectMany(a => a.Place
                    .Select(b => new {a.Intensity, b.Prefecture, b.Place}))
                .GroupBy(a => a.Prefecture)
                .Select(a => new EarthquakeInformation.P2PShindoInformation
                {
                    Prefecture = a.Key, Place = a.Select(b =>
                        new EarthquakeInformation.P2PShindoPlace {Intensity = b.Intensity, Place = b.Place})
                });


        /// <summary>
        /// <see cref="EarthquakeInformation"/> に含まれている震度情報を、P2P地震情報の形式に変換します。
        /// </summary>
        /// <param name="info">震度情報が含まれている震度情報</param>
        /// <returns>P2P地震情報の形式に変換された震度情報</returns>
        public static IEnumerable<EarthquakeInformation.P2PShindoInformation> ToP2PShindoInformation(
            EarthquakeInformation info)
            => ToP2PShindoInformation(info.Shindo);
    }
}
