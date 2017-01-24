using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using NLog;

namespace Mlux.Lib
{
    public delegate void MonitorEvent(object sender, EventArgs e);

    public class TimeKeeper : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly TimeProfile _profile;
        private Thread _timeKeeperThread;
        public event MonitorEvent NodeElapsed;
        public event MonitorEvent CurrentChanged;

        private readonly Monitors _allMonitors;
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private readonly AutoResetEvent _runNow = new AutoResetEvent(false);

        public int CurrentBrightness { get; private set; }
        public int CurrentTemperature { get; private set; }

        public TimeSpan GetRemainingUntilNextNode(DateTime untill)
        {
            var now = TimeUtil.GetRelativeTime(untill);
            var next = Next().TimeOfDay;
            var diff = next - now;
            if (diff < TimeSpan.Zero)
            {
                diff = diff.Add(TimeSpan.FromDays(1));
            }
            return diff;
        }

        public TimeKeeper(TimeProfile profile)
        {
            _profile = profile;

            var monitors = new List<ITemperatureMonitor>();
            foreach (var monitor in Monitors.Create())
            {
                var wrappedMonitor = new LerpedValuesMonitor(new CachedValuesMonitor(monitor));
                monitors.Add(wrappedMonitor);
                wrappedMonitor.CurrentChanged += WrappedMonitor_CurrentChanged;
            }

            _allMonitors = new Monitors(monitors);
        }

        private void WrappedMonitor_CurrentChanged(object sender, EventArgs e)
        {
            CurrentBrightness = _allMonitors.GetBrightness();
            CurrentTemperature = _allMonitors.GetTemperature();
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Start()
        {
            if (_timeKeeperThread != null) throw new ApplicationException($"{GetType()} already started");

            _timeKeeperThread = new Thread(TimeKeeperMainThread) { Name = "TimeKeeper main thread" };
            _timeKeeperThread.Start();
        }

        private void TimeKeeperMainThread()
        {
            var cancel = _cancelSource.Token;

            while (true)
            {
                var relTime = TimeUtil.GetRelativeTime(TimeProvider.Now);
                var previous = Previous();
                var next = Next();

                var brightness = (int)LinearNodeInterpolation.Interpolate(relTime, previous, next, NodeProperty.Brightness);
                var temperature = (int)LinearNodeInterpolation.Interpolate(relTime, previous, next, NodeProperty.ColorTemperature);
                temperature -= temperature % 10;

                _allMonitors.SetBrightness(brightness);
                _allMonitors.SetTemperature(temperature);

                CurrentChanged?.Invoke(this, EventArgs.Empty);
                NodeElapsed?.Invoke(this, EventArgs.Empty);

                var result = WaitHandle.WaitAny(new[] { cancel.WaitHandle, _runNow }, TimeSpan.FromSeconds(10));

                if (result == 0) break; // Stop
            }

        }

        public TimeNode Previous()
        {
            var relTime = TimeUtil.GetRelativeTime(TimeProvider.Now);

            // Find the last node before the given time. If no nodes are found which are after the given time, return the last of the previous day.
            var last = _profile.Nodes.LastOrDefault(node => node.TimeOfDay <= relTime);
            return last ?? _profile.Nodes.Last();
        }

        public TimeNode Next()
        {
            var relTime = TimeUtil.GetRelativeTime(TimeProvider.Now);

            // Find the first node after given time. If no nodes are found which are after the given time, return the first of the next day.
            var first = _profile.Nodes.FirstOrDefault(node => node.TimeOfDay > relTime);
            return first ?? _profile.Nodes.First();
        }

        public void Skip()
        {
            // Make sure CurrentRemaining is updated
            _runNow.Set();

            // Now travel to the future
            TimeProvider.TimeTravel(TimeProvider.Now.Add(GetRemainingUntilNextNode(TimeProvider.Now)));

            // Update brightness/temperature (and remaining again)
            _runNow.Set();
        }

        public void Dispose()
        {
            _cancelSource.Cancel();

            foreach (var monitor in _allMonitors.GetMonitors().OfType<LerpedValuesMonitor>())
            {
                monitor.Dispose();
            }

            Thread.Sleep(100);
            _allMonitors.Reset();
        }
    }
}
