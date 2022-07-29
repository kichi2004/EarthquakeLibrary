using System;
using System.Diagnostics;

namespace EarthquakeLibrary;

/// <summary>
/// 震度を表すクラスです。
/// </summary>
[DebuggerDisplay("{" + nameof(LongString) + "}")]
public class Intensity : IEquatable<Intensity>, IComparable<Intensity>
{
    /// <summary>
    /// 震度不明
    /// </summary>
    public static Intensity Unknown => new("不明", "震度不明", -1);
    /// <summary>
    /// 震度1未満
    /// </summary>
    public static Intensity Int0 => new("0", "震度0", 0);
    /// <summary>
    /// 震度1
    /// </summary>
    public static Intensity Int1 => new("1", "震度1", 1);
    /// <summary>
    /// 震度2
    /// </summary>
    public static Intensity Int2 => new("2", "震度2", 2);
    /// <summary>
    /// 震度3
    /// </summary>
    public static Intensity Int3 => new("3", "震度3", 3);
    /// <summary>
    /// 震度4
    /// </summary>
    public static Intensity Int4 => new("4", "震度4", 4);
    /// <summary>
    /// 震度5弱
    /// </summary>
    public static Intensity Int5Minus => new("5-", "震度5弱", 5);
    /// <summary>
    /// 震度5強
    /// </summary>
    public static Intensity Int5Plus => new("5+", "震度5強", 6);
    /// <summary>
    /// 震度6弱
    /// </summary>
    public static Intensity Int6Minus => new("6-", "震度6弱", 7);
    /// <summary>
    /// 震度6強
    /// </summary>
    public static Intensity Int6Plus => new("6+", "震度6強", 8);
    /// <summary>
    /// 震度7
    /// </summary>
    public static Intensity Int7 => new("7", "震度7", 9);

    private Intensity(string shortString, string longs, int enumOrder)
    {
        EnumOrder = enumOrder;
        ShortString = shortString;
        LongString = longs;
    }

    /// <summary>
    /// 震度の順番(震度0:0、震度7:9)
    /// </summary>
    public int EnumOrder { get; }
    /// <summary>
    /// 短い文字列
    /// </summary>
    public string ShortString { get; }
    /// <summary>
    /// 長い文字列
    /// </summary>
    public string LongString { get; }

    /// <summary>
    /// 震度実測値を <seealso cref="Intensity"/> に変換します。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Intensity FromValue(float value)
    {
        if (value < 0.5) return Int0;
        if (value < 1.5) return Int1;
        if (value < 2.5) return Int2;
        if (value < 3.5) return Int3;
        if (value < 4.5) return Int4;
        if (value < 5.0) return Int5Minus;
        if (value < 5.5) return Int5Plus;
        if (value < 6.0) return Int6Minus;
        return value < 6.5 ? Int6Plus : Int7;
    }

    /// <summary>
    /// 文字列の震度への変換を試みます。
    /// </summary>
    /// <param name="s">文字列</param>
    /// <param name="intensity">震度</param>
    /// <returns>成功したかどうか</returns>
    public static bool TryParse(string s, out Intensity intensity)
    {
        intensity = s?.Replace("震度", "") switch
        {
            "0" or "０" => Int0,
            "1" or "１" => Int1,
            "2" or "２" => Int2,
            "3" or "３" => Int3,
            "4" or "４" => Int4,
            "5-" or "5弱" or "５弱" => Int5Minus,
            "5+" or "5強" or "５強" => Int5Plus,
            "6-" or "6弱" or "６弱" => Int6Minus,
            "6+" or "6強" or "６強" => Int6Plus,
            "7" or "７" => Int7,
            "不明" => Unknown,
            _ => null
        };
        return intensity is not null;
    }

    /// <summary>
    /// 文字列を震度に変換します。
    /// </summary>
    /// <param name="s">文字列</param>
    /// <exception cref="FormatException"/>
    /// <returns>変換された震度</returns>
    public static Intensity Parse(string s)
    {
        return s?.Replace("震度", "") switch
        {
            "0" or "０" => Int0,
            "1" or "１" => Int1,
            "2" or "２" => Int2,
            "3" or "３" => Int3,
            "4" or "４" => Int4,
            "5-" or "5弱" or "５弱" => Int5Minus,
            "5+" or "5強" or "５強" => Int5Plus,
            "6-" or "6弱" or "６弱" => Int6Minus,
            "6+" or "6強" or "６強" => Int6Plus,
            "7" or "７" => Int7,
            "不明" => Unknown,
            _ => throw new FormatException()
        };
    }

    public static explicit operator Intensity(int val)
    {
        return val switch
        {
            0 => Int0,
            1 => Int1,
            2 => Int2,
            3 => Int3,
            4 => Int4,
            5 => Int5Minus,
            6 => Int5Plus,
            7 => Int6Plus,
            8 => Int6Plus,
            9 => Int7,
            _ => throw new ArgumentException(nameof(val))
        };
    }

    public static explicit operator int(Intensity intensity)
    {
        if (intensity == null) throw new ArgumentNullException(nameof(intensity));
        return intensity.EnumOrder;
    }

    public static bool operator ==(Intensity intensity1, Intensity intensity2)
    {
        return intensity1?.EnumOrder == intensity2?.EnumOrder;
    }

    public static bool operator !=(Intensity intensity1, Intensity intensity2)
    {
        return intensity1?.EnumOrder != intensity2?.EnumOrder;
    }

    public static bool operator <(Intensity intensity1, Intensity intensity2) 
        => intensity1?.EnumOrder < intensity2?.EnumOrder;

    public static bool operator >(Intensity intensity1, Intensity intensity2)
        => intensity1?.EnumOrder > intensity2?.EnumOrder;
    public static bool operator <=(Intensity intensity1, Intensity intensity2)
        => intensity1?.EnumOrder <= intensity2?.EnumOrder;

    public static bool operator >=(Intensity intensity1, Intensity intensity2)
        => intensity1?.EnumOrder >= intensity2?.EnumOrder;

    /// <inheritdoc />
    public bool Equals(Intensity other)
    {
        return other != null && EnumOrder == other.EnumOrder || ReferenceEquals(this, other);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Intensity)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return EnumOrder;
    }

    /// <inheritdoc />
    public int CompareTo(Intensity other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return EnumOrder.CompareTo(other.EnumOrder);
    }
}