using System;
using System.Collections.Generic;

namespace EarthquakeLibrary.Extensions
{
    /// <summary>
    /// 拡張メソッド
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// IList 型のインスタンスの各要素に対して、指定された処理を逆順に実行します
        /// </summary>
        public static void ForEachReverse<T>(this IList<T> self, Action<T> act)
        {
            for (var i = self.Count - 1; 0 <= i; i--)
            {
                act(self[i]);
            }
        }

        /// <summary>
        /// IList 型のインスタンスの各要素に対して、指定された処理を逆順に実行します
        /// </summary>
        public static void ForEachReverse<T>(this IList<T> self, Action<T, int> act)
        {
            for (var i = self.Count - 1; 0 <= i; i--)
            {
                act(self[i], i);
            }
        }

        /// <summary>
        /// 重複している要素を抽出して返します
        /// </summary>
        public static IEnumerable<T> GetDistinct<T>(this IList<T> self)
        {
            var uniqueList = new HashSet<T>();

            foreach (var n in self)
            {
                if (!uniqueList.Add(n))
                    yield return n;

            }
        }
    }
}
