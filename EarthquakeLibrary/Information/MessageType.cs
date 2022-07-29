using System;
using System.ComponentModel;

namespace EarthquakeLibrary.Information;

/// <summary>
/// 情報の種類
/// </summary>
[Flags]
public enum MessageType
{
    /// <summary>
    /// 津波警報等（大津波警報・津波警報あるいは津波注意報）を発表中です。
    /// </summary>
    [Description("津波警報等（大津波警報・津波警報あるいは津波注意報）を発表中です。")]
    TsunamiInformation = 1,

    /// <summary>
    /// この地震により、日本の沿岸では若干の海面変動があるかもしれませんが、被害の心配はありません。
    /// </summary>
    [Description("この地震により、日本の沿岸では若干の海面変動があるかもしれませんが、被害の心配はありません。")]
    SeaLevelChange = 1 << 1,

    /// <summary>
    /// この地震による津波の心配はありません。
    /// </summary>
    [Description("この地震による津波の心配はありません。")] NoTsunami = 1 << 2,

    /// <summary>
    /// 震源が海底の場合、津波が発生するおそれがあります。
    /// </summary>
    [Description("震源が海底の場合、津波が発生するおそれがあります。")]
    TsunamiMayOccurIfEpicenterIsSea = 1 << 3,

    /// <summary>
    /// 今後の情報に注意してください。
    /// </summary>
    [Description("今後の情報に注意してください。")] WarnToNextInfo = 1 << 4,

    /// <summary>
    /// 太平洋の広域に津波発生の可能性があります。
    /// </summary>
    [Description("太平洋の広域に津波発生の可能性があります。")] TsunamiMayOccurInWideAreaOfThePacificOcean = 1 << 5,

    /// <summary>
    /// 太平洋で津波発生の可能性があります。
    /// </summary>
    [Description("太平洋で津波発生の可能性があります。")] TsunamiMayOccurInThePacificOcean = 1 << 6,

    /// <summary>
    /// 北西太平洋で津波発生の可能性があります。
    /// </summary>
    [Description("北西太平洋で津波発生の可能性があります。")] TsunamiMayOccurInNorthWestPacificOcean = 1 << 7,

    /// <summary>
    /// インド洋の広域に津波発生の可能性があります。
    /// </summary>
    [Description("インド洋の広域に津波発生の可能性があります。")]
    TsunamiMayOccurInWideAreaOfIndianTheOcean = 1 << 8,

    /// <summary>
    /// インド洋で津波発生の可能性があります。
    /// </summary>
    [Description("インド洋で津波発生の可能性があります。")] TsunamiMayOccurInTheIndianOcean = 1 << 9,

    /// <summary>
    /// 震源の近傍で津波発生の可能性があります。
    /// </summary>
    [Description("震源の近傍で津波発生の可能性があります。")] TsunamiMayOccurNearTheEpicenter = 1 << 10,

    /// <summary>
    /// 震源の近傍で小さな津波発生の可能性がありますが、被害をもたらす津波の心配はありません。
    /// </summary>
    [Description("震源の近傍で小さな津波発生の可能性がありますが、被害をもたらす津波の心配はありません。")]
    LittleTsunamiMayOccurNearTheEpicenter_ButNoDamage = 1 << 11,

    /// <summary>
    /// 一般的に、この規模の地震が海域の浅い領域で発生すると、津波が発生することがあります。
    /// </summary>
    [Description("一般的に、この規模の地震が海域の浅い領域で発生すると、津波が発生することがあります。")]
    TsunamiMayOccurIfAboutThisScaleEarthquakeOccursInShallowSeaInGeneral = 1 << 12,

    /// <summary>
    /// 日本への津波の有無については現在調査中です。
    /// </summary>
    [Description("日本への津波の有無については現在調査中です。")]
    InvestigatingForTsunamiInJapan = 1 << 13,

    /// <summary>
    /// この地震による日本への津波の影響はありません。
    /// </summary>
    [Description("この地震による日本への津波の影響はありません。")]
    NoTsunamiInJapan = 1 << 14,

    /// <summary>
    /// この地震について、緊急地震速報を発表しています。
    /// </summary>
    [Description("この地震について、緊急地震速報を発表しています。")]
    EEW = 1 << 15,

    /// <summary>
    /// この地震について、緊急地震速報を発表しています。この地震の最大震度は２でした。
    /// </summary>
    [Description("この地震について、緊急地震速報を発表しています。この地震の最大震度は２でした。")]
    EEW_Int2 = 1 << 16,

    /// <summary>
    /// この地震について、緊急地震速報を発表しています。この地震の最大震度は１でした。
    /// </summary>
    [Description("この地震について、緊急地震速報を発表しています。この地震の最大震度は１でした。")]
    EEW_Int1 = 1 << 17,

    /// <summary>
    /// この地震について、緊急地震速報を発表しています。この地震で震度１以上は観測されていません。
    /// </summary>
    [Description("この地震について、緊急地震速報を発表しています。この地震で震度１以上は観測されていません。")]
    EEW_NoShake = 1 << 18,

    /// <summary>
    /// この地震で緊急地震速報を発表しましたが、強い揺れは観測されませんでした。
    /// </summary>
    [Description("この地震で緊急地震速報を発表しましたが、強い揺れは観測されませんでした。")]
    EEW_NoStrongShake = 1 << 19
}