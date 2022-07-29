using System;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeLibrary.Core;

public static class LibCore
{
    internal static async Task<string> DownloadString(Uri uri)
    {
        using var http = new HttpClient();
        return await http.GetStringAsync(uri);
    }
    internal static async Task<string> DownloadString(string url)
    {
        using var http = new HttpClient();
        return await http.GetStringAsync(url);
    }
}