using System;
using System.Globalization;
using System.Net;
using System.Timers;

namespace EarthquakeLibrary.EEW
{
    /// <summary>
    /// 緊急地震速報の監視を行うクラスです。
    /// </summary>
    public class EewWatcher
    {
        /// <summary>
        /// EEWWatcherを初期化します。
        /// </summary>
        /// <param name="interval">取得間隔（秒）</param>
        public EewWatcher(ushort interval) : this()
            => _timer.Interval = interval * 1000;
        /// <summary>
        /// EEWWatcherを初期化します。
        /// </summary>
        public EewWatcher()
        {
            _timer = new Timer { Interval = 1000 };
            SetTime();
            _timer.Elapsed += Timer_Elapsed;
        }

        private Timer _timer;
        private int _c;
        private DateTime _time;
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (_c == (int)_timer.Interval / 5 * 18) SetTime();

            _c++;
        }
        /// <summary>
        /// 時刻を修正します。
        /// </summary>
        public void SetTime()
        {
            try
            {
                _time = DateTime.ParseExact(new WebClient().DownloadString("http://ntp-a1.nict.go.jp/cgi-bin/time"),
                    "ddd MMM dd HH:mm:ss yyyy JST\n", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch
            {
                // Do nothing now
            }
        }
    }
}