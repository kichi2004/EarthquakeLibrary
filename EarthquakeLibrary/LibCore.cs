using System;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeLibrary.Core
{
    public class LibCore
    {
        internal static async Task<string> DownloadString(Uri uri)
        {
            using (var wc = new WebClient{Encoding = Encoding.UTF8,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)})
            {
                return await wc.DownloadStringTaskAsync(uri);
            }
        }
        internal static async Task<string> DownloadString(string url)
        {
            using (var wc = new WebClient{Encoding = Encoding.UTF8,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)})
            {
                return await wc.DownloadStringTaskAsync(url);
            }
        }
    }
}
