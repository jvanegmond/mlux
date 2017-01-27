using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Mlux.Lib.Time
{
    public class DefaultTimeProfile
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static TimeProfile Create()
        {

            Log.Info("Creating default time profile");

            var result = new TimeProfile();

            var wakeUpTime = new TimeNode(TimeSpan.FromHours(7));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.AddNode(wakeUpTime);

            var morning = new TimeNode(TimeSpan.FromHours(8));
            morning.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            morning.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.AddNode(morning);

            var srsModeOver = new TimeNode(TimeSpan.FromHours(17));
            srsModeOver.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            srsModeOver.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.AddNode(srsModeOver);

            var afterDinner = new TimeNode(TimeSpan.FromHours(19));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.Brightness, 40));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 5000));
            result.AddNode(afterDinner);

            var bedTime = new TimeNode(TimeSpan.FromHours(22));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.AddNode(bedTime);

            return result;
        }
    }
}
