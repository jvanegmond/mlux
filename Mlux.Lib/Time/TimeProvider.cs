using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Mlux.Lib.Time
{
    public static class TimeProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static DateTime _timeTravelDestination = DateTime.MinValue;

        public static DateTime Now
        {
            get
            {
                var now = DateTime.Now;
                if (_timeTravelDestination > now)
                {
                    return _timeTravelDestination;
                }
                return now;
            }
        }

        public static void TimeTravel(DateTime destination)
        {
            if (destination < Now) return; // Check if we are not breaking the laws of physics

            while (destination - DateTime.Now > TimeSpan.FromDays(1))
            {
                destination = destination.AddDays(-1);
            }

            Log.Debug($"Traveling to future: {_timeTravelDestination}");
            _timeTravelDestination = destination;
        }
    }
}
