using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EarthquakeLibrary.Core;

namespace EarthquakeLibrary.Information
{
    /// <inheritdoc />
    /// <summary>地震情報</summary>
    public class EarthquakeInformation : OldEarthquakeInformation
    {
        /// <summary>
        /// 
        /// </summary>
        protected EarthquakeInformation()
        {
        }

        internal EarthquakeInformation(string origin_time, string epicenter, string magnitude,
            string intensity, string lat, string lon,
            string depth, string message, string detailUrl, string regionUrl)
        {
            if (origin_time == "---") OriginTime = null;
            else if (DateTime.TryParseExact(origin_time, "yyyy年M月d日 H時m分ごろ", DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.NoCurrentDateDefault, out var originTime)) OriginTime = originTime;
            else
                Console.Error.WriteLine($"'{origin_time}' は有効な DateTime に変換されませんでした。");

            //this.Announced_time = DateTime.ParseExact(announced_time, "yyyy年M月d日 H時m分",
            //    DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault);

            Epicenter = epicenter == "---" || epicenter == "" ? null : epicenter;

            Magnitude = magnitude == "---" ? (float?) null : float.Parse(magnitude);

            MaxIntensity = Intensity.TryParse(intensity, out var i) ? i : Intensity.Unknown;

            Location = lat != null && lon != null ? new Location(float.Parse(lat), float.Parse(lon)) : null;

            switch (depth)
            {
                case "---":
                    Depth = null;
                    break;
                case "ごく浅い":
                    Depth = 0;
                    break;
                default:
                    Depth = short.Parse(depth.Replace("km", "").Replace("以上", ""));
                    break;
            }

            DetailImageUrl = string.IsNullOrEmpty(detailUrl) ? null : new Uri(detailUrl);
            RegionImageUrl = string.IsNullOrEmpty(regionUrl) ? null : new Uri(regionUrl);

            var message2 = (MessageType) 0;
            if (message.Contains("津波警報等")) message2 |= MessageType.TsunamiInformation;
            if (message.Contains("日本の沿岸では若干の海面変動")) message2 |= MessageType.SeaLevelChange;
            if (message.Contains("この地震による津波の心配はありません。")) message2 |= MessageType.NoTsunami;
            if (message.Contains("震源が海底の場合")) message2 |= MessageType.TsunamiMayOccurIfEpicenterIsSea;
            if (message.Contains("今後の情報に注意")) message2 |= MessageType.WarnToNextInfo;
            if (message.Contains("太平洋の広域")) message2 |= MessageType.TsunamiMayOccurInWideAreaOfThePacificOcean;
            if (message.Contains("太平洋で津波発生の可能性"))
            {
                message2 |= message.Contains("北西大西洋")
                    ? MessageType.TsunamiMayOccurInNorthWestPacificOcean
                    : MessageType.TsunamiMayOccurInThePacificOcean;
            }

            if (message.Contains("インド洋の広域")) message2 |= MessageType.TsunamiMayOccurInWideAreaOfIndianTheOcean;
            if (message.Contains("インド洋")) message2 |= MessageType.TsunamiMayOccurInTheIndianOcean;
            if (message.Contains("震源の近傍で津波発生")) message2 |= MessageType.TsunamiMayOccurNearTheEpicenter;
            if (message.Contains("震源の近傍で小さな津波"))
                message2 |= MessageType.LittleTsunamiMayOccurNearTheEpicenter_ButNoDamage;
            if (message.Contains("一般的"))
                message2 |= MessageType.TsunamiMayOccurIfAboutThisScaleEarthquakeOccursInShallowSeaInGeneral;
            if (message.Contains("現在調査中")) message2 |= MessageType.InvestigatingForTsunamiInJapan;
            if (message.Contains("日本への津波の影響は")) message2 |= MessageType.NoTsunamiInJapan;
            if (message.Contains("緊急地震速報"))
            {
                var b = false;
                if (message.Contains("最大震度は２")) message2 |= MessageType.EEW_Int2;
                else if (message.Contains("最大震度は１")) message2 |= MessageType.EEW_Int1;
                else if (message.Contains("震度１以上は")) message2 |= MessageType.EEW_NoShake;
                else b = true;
                if (message.Contains("強い揺れは観測")) message2 |= MessageType.EEW_NoStrongShake;
                else if (b) message2 |= MessageType.EEW;
            }

            Message = message2;
            Info_url = null;
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
        public MessageType Message { get; set; }

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
            get
            {
                //震度速報
                if (!Shindo.Any()) return InformationType.UnknownSesimic;
                if (Epicenter == null)
                    return InformationType.SesimicInfo;
                return DetailImageUrl == null && MaxIntensity >= Intensity.Int3
                    ? InformationType.EpicenterInfo
                    : InformationType.EarthquakeInfo;

                //震度不明
            }
        }

        public bool IsSameInformation(EarthquakeInformation other)
        {
            if (other is null) return false;

            return OriginTime == other.OriginTime && Epicenter == other.Epicenter &&
                   (Magnitude == null && other.Magnitude == null ||
                    Magnitude != null && other.Magnitude != null &&
                    Math.Abs(Magnitude.Value - other.Magnitude.Value) < 0.00001) &&
                   Depth == other.Depth &&
                   Shindo.SequenceEqual(other.Shindo) &&
                   InformationType == other.InformationType;
        }
        
        /// <summary>
        /// 震度と ShindoPlace の組を表します。
        /// </summary>
        public class ShindoInformation : IEquatable<ShindoInformation>
        {
            /// <summary>
            /// 震度の文字列と ShindoPlace で ShindoInformation クラスを初期化します。
            /// </summary>
            /// <param name="shindo"></param>
            /// <param name="place"></param>
            public ShindoInformation(string shindo, IEnumerable<ShindoPlace> place)
            {
                if (!Intensity.TryParse(shindo, out _)) {
                    Console.WriteLine();
                }
                Intensity = Intensity.Parse(shindo);
                Place = place;
            }

            /// <summary>
            /// 震度
            /// </summary>
            public Intensity Intensity { get; set; }

            /// <summary>
            /// 観測地点
            /// </summary>
            public IEnumerable<ShindoPlace> Place { get; set; }

            

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return Intensity.EnumOrder;
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (!(obj is ShindoInformation b)) return false;
                
                return Intensity == b.Intensity && Place.SequenceEqual(b.Place);
            }


            /// <inheritdoc />
            public bool Equals(ShindoInformation other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Intensity, other.Intensity) && Place.SequenceEqual(other.Place);
            }
        }

        /// <summary>
        /// 都道府県と観測点の一覧の組を表します。
        /// </summary>
        public class ShindoPlace
        {
            /// <summary>
            /// 都道府県名と観測点の一覧で ShindoPlace を初期化します。
            /// </summary>
            /// <param name="prefcture">都道府県</param>
            /// <param name="place">観測点の一覧</param>
            public ShindoPlace(string prefcture, IEnumerable<string> place)
            {
                Prefecture = prefcture;
                Place = place;
            }

            /// <summary>
            /// 都道府県
            /// </summary>
            public string Prefecture { get; set; }

            /// <summary>
            /// 観測 市区町村
            /// </summary>
            public IEnumerable<string> Place { get; set; }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (!(obj is ShindoPlace b)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (Prefecture != b.Prefecture) return false;
                var placeA = Place.ToArray();
                var placeB = b.Place.ToArray();
                if (placeA.Length != placeB.Length) return false;
                Array.Sort(placeA);
                Array.Sort(placeB);
                return placeA.Zip(placeB, (s1, s2) => (s1, s2)).All(x => x.s1 == x.s2);
            }

            public override int GetHashCode()
            {
                var hashCode = Prefecture.GetHashCode();
                return hashCode;
            }

        }

#pragma warning disable 1591
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
#pragma warning restore 1591
    }
}