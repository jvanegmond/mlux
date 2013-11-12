using System;

namespace Mlux.Lib.Time
{
    public static class TimeUtil
    {
        public static TimeSpan GetRelativeTime(DateTime time)
        {
            return new TimeSpan(time.Hour, time.Minute, time.Second);
        }
    }
}
