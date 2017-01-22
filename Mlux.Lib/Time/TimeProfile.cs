using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mlux.Lib.Time
{
    [Serializable]
    public class TimeProfile
    {
        public const int MaxTemperature = 6500;
        public const int MinTemperature = 3200;
        public const int MinBrightness = 0;
        public const int MaxBrightness = 100;

        public string Name { get; set; }

        // node --------------- node ----------- time ------------- node ------------ node
        // current node   =   last node before given time
        // next node      =   first node after given time

		
        public List<TimeNode> Nodes { get; private set; }

        public TimeProfile()
        {
            Nodes = new List<TimeNode>();
        }

        public TimeNode GetCurrentNode(DateTime time)
        {
            var relTime = TimeUtil.GetRelativeTime(time);
            return GetCurrentNode(relTime);
        }

        public TimeNode GetCurrentNode(TimeSpan relTime)
        {
            // find the last node before the given time
            var last = Nodes.LastOrDefault(node => node.TimeOfDay < relTime);
            if (last != null) return last;

            // if no nodes are found which are after the given time, return the last of the previous day
            return Nodes.LastOrDefault();
        }

        public TimeNode GetNextNode(DateTime time)
        {
            var relTime = TimeUtil.GetRelativeTime(time);
            return GetNextNode(relTime);
        }

        public TimeNode GetNextNode(TimeSpan relTime)
        {
            // find the first node after given time
            var last = Nodes.FirstOrDefault(node => node.TimeOfDay > relTime);
            if (last != null) return last;

            // if no nodes are found which are after the given time, return the first of the next day
            return Nodes.FirstOrDefault();
        }

        public override string ToString()
        {
            return $"{GetType()} with {Nodes.Count} nodes";
        }

        public object GetCurrentValue(DateTime time, string nodePropertyName)
        {
            var currentNode = GetCurrentNode(time);
            if (currentNode == null) return null;

            var nextNode = GetNextNode(time);
            if (nextNode == null) return null;

            return LinearNodeInterpolation.Interpolate(time, currentNode, nextNode, nodePropertyName);
        }

        public static TimeProfile GetDefault()
        {
            var result = new TimeProfile();

            var wakeUpTime = new TimeNode(TimeSpan.FromHours(7));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.Nodes.Add(wakeUpTime);

            var morning = new TimeNode(TimeSpan.FromHours(8));
            morning.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            morning.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.Nodes.Add(morning);

            var srsModeOver = new TimeNode(TimeSpan.FromHours(17));
            srsModeOver.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            srsModeOver.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.Nodes.Add(srsModeOver);

            var afterDinner = new TimeNode(TimeSpan.FromHours(19));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.Brightness, 40));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 5000));
            result.Nodes.Add(afterDinner);

            var bedTime = new TimeNode(TimeSpan.FromHours(22));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.Nodes.Add(bedTime);

            return result;
        }
    }
}
