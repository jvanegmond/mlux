using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mlux.Lib.Time
{
    public class TimeProfileNodeAddedEvent : EventArgs
    {
        public TimeNode Node { get; set; }

        public TimeProfileNodeAddedEvent(TimeNode node)
        {
            Node = node;
        }
    }

    public delegate void TimeProfileNodeAddedDelegate(object sender, TimeProfileNodeAddedEvent e);

    [Serializable]
    public class TimeProfile
    {
        public const int MaxTemperature = 6500;
        public const int MinTemperature = 3200;
        public const int MinBrightness = 0;
        public const int MaxBrightness = 100;

        public event TimeProfileNodeAddedDelegate NodeAdded;

        public string Name { get; set; }

        // node --------------- node ----------- time ------------- node ------------ node
        // current node   =   last node before given time
        // next node      =   first node after given time
        public List<TimeNode> Nodes { get; }

        public TimeProfile()
        {
            Nodes = new List<TimeNode>();
        }

        public TimeNode Previous(DateTime now)
        {
            var relTime = TimeUtil.GetRelativeTime(now);

            // Find the last node before the given time. If no nodes are found which are after the given time, return the last of the previous day.
            var last = Nodes.LastOrDefault(node => node.TimeOfDay <= relTime);
            return last ?? Nodes.Last();
        }

        public TimeNode Next(DateTime now)
        {
            var relTime = TimeUtil.GetRelativeTime(now);

            // Find the first node after given time. If no nodes are found which are after the given time, return the first of the next day.
            var first = Nodes.FirstOrDefault(node => node.TimeOfDay > relTime);
            return first ?? Nodes.First();
        }

        public override string ToString()
        {
            return $"{GetType()} with {Nodes.Count} nodes";
        }

        public object GetCurrentValue(TimeNode previous, TimeNode next, TimeSpan time, string nodePropertyName)
        {
            return LinearNodeInterpolation.Interpolate(time, previous, next, nodePropertyName);
        }

        public void AddNode(TimeNode node)
        {
            Nodes.Add(node);
            NodeAdded?.Invoke(this, new TimeProfileNodeAddedEvent(node));
        }
    }
}
